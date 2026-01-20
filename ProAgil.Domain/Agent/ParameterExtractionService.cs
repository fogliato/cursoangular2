using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Service responsible for extracting parameters from messages using AI
    /// </summary>
    public class ParameterExtractionService
    {
        private readonly GeminiApiService _geminiService;

        public ParameterExtractionService()
        {
            _geminiService = new GeminiApiService();
        }

        public async Task<object[]> ExtractMethodParameters(
            string message,
            string controllerName,
            string actionName,
            ParameterInfo[] parameters,
            IChatCompletionService chatService
        )
        {
            var methodParams = new object[parameters.Length];

            if (parameters.Length == 0)
            {
                return methodParams;
            }

            var extractionPrompt = AgentPrompts.BuildExtractionPrompt(
                message,
                controllerName,
                actionName,
                parameters
            );

            var chat = new ChatHistory();
            chat.AddUserMessage(extractionPrompt);
            var extractionResult = await _geminiService.ExecuteWithRetry(chat, chatService);
            var jsonParams = extractionResult.Content?.Trim();

            if (!string.IsNullOrEmpty(jsonParams))
            {
                jsonParams = JsonHelper.CleanJsonResponse(jsonParams);
                var extractedParams = JsonHelper.Deserialize<Dictionary<string, JsonElement>>(
                    jsonParams
                );

                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    if (
                        extractedParams != null
                        && param.Name != null
                        && extractedParams.TryGetValue(param.Name, out var value)
                    )
                    {
                        methodParams[i] = JsonHelper.DeserializeParameter(
                            value,
                            param.ParameterType
                        )!;
                    }
                    else
                    {
                        methodParams[i] = (param.ParameterType.IsValueType
                            ? Activator.CreateInstance(param.ParameterType)
                            : null)!;
                    }
                }
            }

            return methodParams;
        }

        public async Task<object> BuildParametersFromPreviousStep(
            ExecutionStep currentStep,
            object previousStepData,
            Type targetType,
            IChatCompletionService chatService
        )
        {
            var previousDataJson = JsonHelper.Serialize(previousStepData);

            Console.WriteLine(
                $"[ParameterExtractionService] Building parameters for {targetType.Name} from previous step data"
            );

            // If data was already transformed (from TRANSFORM_DATA), use it directly
            var previousElement = JsonHelper.DeserializeToElement(previousDataJson);
            if (previousElement.TryGetProperty("data", out var dataElement))
            {
                var dataJson = JsonHelper.Serialize(dataElement);
                Console.WriteLine($"[ParameterExtractionService] Using already transformed data");
                Console.WriteLine(
                    $"[ParameterExtractionService] JSON sendo deserializado: {dataJson.Substring(0, Math.Min(500, dataJson.Length))}"
                );

                // CRITICAL: Use serialization options that preserve camelCase and properties
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Allows camelCase -> PascalCase
                    WriteIndented = false,
                };

                var deserializedResult = JsonSerializer.Deserialize(dataJson, targetType, options);
                Console.WriteLine(
                    $"[ParameterExtractionService] Deserialization result: {(deserializedResult != null ? "OK" : "NULL")}"
                );

                // Log deserialized properties for debugging
                if (deserializedResult != null)
                {
                    var properties = targetType.GetProperties();
                    foreach (var prop in properties.Take(5))
                    {
                        var value = prop.GetValue(deserializedResult);
                        Console.WriteLine($"[ParameterExtractionService]   {prop.Name} = {value}");
                    }
                }

                return deserializedResult!;
            }

            // If not, use AI to transform
            var typePropertiesDescription = ParameterInfoBuilder.GetTypePropertiesDescription(
                targetType
            );
            var transformPrompt = AgentPrompts.BuildParameterMappingPrompt(
                previousDataJson,
                currentStep.Description ?? string.Empty,
                targetType.Name,
                typePropertiesDescription
            );

            var chat = new ChatHistory();
            chat.AddUserMessage(transformPrompt);
            var result = await _geminiService.ExecuteWithRetry(chat, chatService);
            var jsonResponse = JsonHelper.CleanJsonResponse(result.Content?.Trim());

            return JsonSerializer.Deserialize(jsonResponse, targetType)!;
        }
    }
}
