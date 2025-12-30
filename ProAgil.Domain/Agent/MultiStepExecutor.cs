using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Service responsible for executing multi-step operations and data transformation
    /// </summary>
    public class MultiStepExecutor
    {
        private readonly ControllerInvoker _controllerInvoker;
        private readonly ParameterExtractionService _parameterExtraction;
        private readonly GeminiApiService _geminiService;
        private readonly IChatCompletionService _chatService;

        public MultiStepExecutor(
            ControllerInvoker controllerInvoker,
            ParameterExtractionService parameterExtraction,
            GeminiApiService geminiService,
            IChatCompletionService chatService
        )
        {
            _controllerInvoker = controllerInvoker;
            _parameterExtraction = parameterExtraction;
            _geminiService = geminiService;
            _chatService = chatService;
        }

        public async Task<ExecutionPlan> AnalyzeExecutionPlan(string message)
        {
            var analysisPrompt = AgentPrompts.BuildExecutionAnalysisPrompt(message);

            var chat = new ChatHistory();
            chat.AddUserMessage(analysisPrompt);
            var result = await _geminiService.ExecuteWithRetry(chat, _chatService);
            var jsonResponse = JsonHelper.CleanJsonResponse(result.Content?.Trim());

            try
            {
                var planData = JsonHelper.DeserializeToElement(jsonResponse);
                var isMultiStep = planData.GetProperty("isMultiStep").GetBoolean();

                if (!isMultiStep)
                {
                    return new ExecutionPlan { IsMultiStep = false };
                }

                var plan = new ExecutionPlan { IsMultiStep = true, UserMessage = message };

                if (planData.TryGetProperty("steps", out var stepsElement))
                {
                    foreach (var stepElement in stepsElement.EnumerateArray())
                    {
                        var step = new ExecutionStep
                        {
                            Order = stepElement.GetProperty("order").GetInt32(),
                            Description = stepElement.GetProperty("description").GetString(),
                            Controller = stepElement.GetProperty("controller").GetString(),
                            Action = stepElement.GetProperty("action").GetString(),
                            ParameterSource = stepElement
                                .GetProperty("parameterSource")
                                .GetString(),
                        };

                        if (
                            stepElement.TryGetProperty(
                                "transformation",
                                out var transformationElement
                            )
                        )
                        {
                            step.Transformation = transformationElement.GetString();
                        }

                        plan.Steps.Add(step);
                    }
                }

                return plan;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[MultiStepExecutor] Error analyzing execution plan: {ex.Message}"
                );
                return new ExecutionPlan { IsMultiStep = false };
            }
        }

        public async Task<object> ExecuteMultiStepOperation(ExecutionPlan plan)
        {
            var results = new List<object>();
            object? previousStepData = null;

            foreach (var step in plan.Steps.OrderBy(s => s.Order))
            {
                Console.WriteLine(
                    $"[MultiStepExecutor] Executing step {step.Order}: {step.Description}"
                );

                try
                {
                    var stepResult = await ExecuteSingleStep(
                        step,
                        previousStepData!,
                        plan.UserMessage ?? string.Empty
                    );
                    results.Add(stepResult);

                    // Access properties of anonymous type via dynamic
                    dynamic dynResult = stepResult;
                    previousStepData = dynResult.data;

                    // Check if the step failed (success = false)
                    // ExecuteSingleStep now returns success=false if the API returns an error
                    if (dynResult.success == false)
                    {
                        return new
                        {
                            success = false,
                            message = $"Failure in step {step.Order}: {step.Description}. Details: {dynResult.message ?? "Unknown error"}",
                            results,
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        success = false,
                        message = $"Error in step {step.Order}: {ex.Message}",
                        results,
                    };
                }
            }

            return new
            {
                success = true,
                message = $"Multi-step operation completed successfully ({plan.Steps.Count} steps)",
                data = previousStepData, // Return only the final result
                totalSteps = plan.Steps.Count,
            };
        }

        private async Task<object> ExecuteSingleStep(
            ExecutionStep step,
            object previousStepData,
            string userMessage
        )
        {
            Console.WriteLine($"[MultiStepExecutor] Executing {step.Controller}.{step.Action}");

            // If it's data transformation, process locally
            if (step.Action == "TRANSFORM_DATA")
            {
                return await TransformData(step, previousStepData, userMessage);
            }

            // Find the controller and method
            var (controllerType, method) = _controllerInvoker.FindControllerAndMethod(
                step.Controller ?? string.Empty,
                step.Action ?? string.Empty
            );

            // Extract parameters
            var parameters = method.GetParameters();
            var methodParams = new object[parameters.Length];

            if (parameters.Length > 0)
            {
                if (step.ParameterSource == "previous_step" && previousStepData != null)
                {
                    methodParams[0] = await _parameterExtraction.BuildParametersFromPreviousStep(
                        step,
                        previousStepData,
                        parameters[0].ParameterType,
                        _chatService
                    );
                }
                else
                {
                    methodParams = await _parameterExtraction.ExtractMethodParameters(
                        step.Description ?? string.Empty,
                        step.Controller ?? string.Empty,
                        step.Action ?? string.Empty,
                        parameters,
                        _chatService
                    );
                }
            }

            // LOG PARAMETERS for debugging
            try
            {
                Console.WriteLine(
                    $"[MultiStepExecutor] Parameters for {step.Controller}.{step.Action}: {JsonHelper.Serialize(methodParams)}"
                );
            }
            catch
            { /* ignore serialization errors in logs */
            }

            // Execute the method
            var result = await _controllerInvoker.InvokeController(
                step.Controller ?? string.Empty,
                step.Action ?? string.Empty,
                methodParams
            );

            // Check if the API result indicates failure (IResponseMessage.Success)
            bool apiSuccess = true;
            string? apiMessage = null;

            if (result != null)
            {
                var propSuccess = result
                    .GetType()
                    .GetProperty(
                        "Success",
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                    );
                if (propSuccess != null && propSuccess.PropertyType == typeof(bool))
                {
                    var successVal = propSuccess.GetValue(result) is bool val && val;
                    if (!successVal)
                    {
                        apiSuccess = false;
                        // Try to extract error message
                        var propMessages = result
                            .GetType()
                            .GetProperty(
                                "Messages",
                                BindingFlags.IgnoreCase
                                    | BindingFlags.Public
                                    | BindingFlags.Instance
                            );
                        if (propMessages != null)
                        {
                            apiMessage = JsonHelper.Serialize(propMessages.GetValue(result));
                        }
                    }
                }
            }

            if (!apiSuccess)
            {
                Console.WriteLine(
                    $"[MultiStepExecutor] Step failed. API message: {apiMessage}"
                );
                return new
                {
                    success = false,
                    data = result,
                    message = apiMessage ?? "API returned failure",
                    controller = step.Controller,
                    action = step.Action,
                    step = step.Description,
                };
            }

            return new
            {
                success = true,
                data = result,
                controller = step.Controller,
                action = step.Action,
                step = step.Description,
            };
        }

        private async Task<object> TransformData(
            ExecutionStep step,
            object previousStepData,
            string userMessage
        )
        {
            Console.WriteLine($"[MultiStepExecutor] Transforming data: {step.Transformation}");

            // Check if previousStepData is null or invalid
            if (previousStepData == null)
            {
                Console.WriteLine("[MultiStepExecutor] ERROR: Previous step data is null.");
                return new
                {
                    success = false,
                    message = "Previous step data is null. Check if the previous step was executed correctly.",
                    step = step.Description,
                };
            }

            var previousDataJson = JsonHelper.Serialize(previousStepData);
            var previousElement = JsonHelper.DeserializeToElement(previousDataJson);

            string dataToTransform = previousDataJson;
            
            // If it's an array, get the first element for transformation
            if (previousElement.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine("[MultiStepExecutor] Previous step data is an array.");
                if (previousElement.GetArrayLength() > 0)
                {
                    var firstElement = previousElement[0];
                    dataToTransform = JsonHelper.Serialize(firstElement);
                    Console.WriteLine($"[MultiStepExecutor] Using first element of array for transformation.");
                }
                else
                {
                    Console.WriteLine("[MultiStepExecutor] ERROR: Previous step array is empty.");
                    return new
                    {
                        success = false,
                        message = "Previous step array is empty.",
                        step = step.Description,
                    };
                }
            }
            else if (previousElement.TryGetProperty("data", out JsonElement dataElement))
            {
                // If 'data' is null within the response object
                if (dataElement.ValueKind == JsonValueKind.Null)
                {
                    Console.WriteLine(
                        "[MultiStepExecutor] ERROR: 'data' property from previous step is null."
                    );
                    return new
                    {
                        success = false,
                        message = "No data returned in previous step to be processed.",
                        step = step.Description,
                    };
                }
                
                // If 'data' is an array, get the first element
                if (dataElement.ValueKind == JsonValueKind.Array && dataElement.GetArrayLength() > 0)
                {
                    Console.WriteLine("[MultiStepExecutor] 'data' property is an array, using first element.");
                    dataToTransform = JsonHelper.Serialize(dataElement[0]);
                }
                else
                {
                    dataToTransform = JsonHelper.Serialize(dataElement);
                }
            }

            // Use improved prompt that includes user message for verification
            var transformPrompt = AgentPrompts.BuildDataTransformationPrompt(
                dataToTransform,
                step.Transformation ?? string.Empty,
                step.Description ?? string.Empty,
                userMessage
            );

            var chat = new ChatHistory();
            chat.AddUserMessage(transformPrompt);
            var result = await _geminiService.ExecuteWithRetry(chat, _chatService);
            var transformedJson = JsonHelper.CleanJsonResponse(result.Content?.Trim());

            Console.WriteLine(
                $"[MultiStepExecutor] Transformed data (first 500 chars): {transformedJson.Substring(0, Math.Min(500, transformedJson.Length))}"
            );

            // Check if the result is an array
            var transformedData = JsonHelper.DeserializeToElement(transformedJson);
            bool isArray = transformedData.ValueKind == JsonValueKind.Array;
            Console.WriteLine(
                $"[MultiStepExecutor] Result type: {(isArray ? "Array" : "Object")}"
            );

            // Convert JsonElement to standard CLR objects (List/Dictionary) to ensure correct serialization
            var cleanData = JsonHelper.ConvertJsonElement(transformedData);

            // If it's an array, return the list
            if (isArray)
            {
                return new
                {
                    success = true,
                    data = cleanData,
                    dataType = "Array",
                    transformation = step.Transformation,
                    step = step.Description,
                    isArray = true,
                };
            }

            // Validate that it doesn't contain RPs (only for single objects)
            if (
                transformedData.TryGetProperty("rPs", out _)
                || transformedData.TryGetProperty("RPs", out _)
                || transformedData.TryGetProperty("solicitacaoCriacaoRps", out _)
            )
            {
                Console.WriteLine(
                    "[MultiStepExecutor] WARNING: Transformed JSON contains RPs, removing..."
                );

                transformedJson = JsonHelper.Serialize(cleanData);
                Console.WriteLine(
                    $"[MultiStepExecutor] Clean JSON (first 500 chars): {transformedJson.Substring(0, Math.Min(500, transformedJson.Length))}"
                );
            }

            return new
            {
                success = true,
                data = cleanData,
                dataType = "EventNewModel",
                transformation = step.Transformation,
                step = step.Description,
            };
        }
    }
}
