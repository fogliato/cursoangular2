using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Serviço responsável por executar operações multi-etapa e transformação de dados
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
                    $"[MultiStepExecutor] Erro ao analisar plano de execução: {ex.Message}"
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
                    $"[MultiStepExecutor] Executando passo {step.Order}: {step.Description}"
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

                    // Verificar se o passo falhou (success = false)
                    // O ExecuteSingleStep agora retorna success=false se a API retornar erro
                    if (dynResult.success == false)
                    {
                        return new
                        {
                            success = false,
                            message = $"Falha no passo {step.Order}: {step.Description}. Detalhes: {dynResult.message ?? "Erro desconhecido"}",
                            results,
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new
                    {
                        success = false,
                        message = $"Erro no passo {step.Order}: {ex.Message}",
                        results,
                    };
                }
            }

            return new
            {
                success = true,
                message = $"Operação multi-etapa concluída com sucesso ({plan.Steps.Count} passos)",
                data = previousStepData, // Retornar apenas o resultado final
                totalSteps = plan.Steps.Count,
            };
        }

        private async Task<object> ExecuteSingleStep(
            ExecutionStep step,
            object previousStepData,
            string userMessage
        )
        {
            Console.WriteLine($"[MultiStepExecutor] Executando {step.Controller}.{step.Action}");

            // Se for transformação de dados, processar localmente
            if (step.Action == "TRANSFORM_DATA")
            {
                return await TransformData(step, previousStepData, userMessage);
            }

            // Encontrar o controller e método
            var (controllerType, method) = _controllerInvoker.FindControllerAndMethod(
                step.Controller ?? string.Empty,
                step.Action ?? string.Empty
            );

            // Extrair parâmetros
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
                    $"[MultiStepExecutor] Parâmetros para {step.Controller}.{step.Action}: {JsonHelper.Serialize(methodParams)}"
                );
            }
            catch
            { /* ignore serialization errors in logs */
            }

            // Executar o método
            var result = await _controllerInvoker.InvokeController(
                step.Controller ?? string.Empty,
                step.Action ?? string.Empty,
                methodParams
            );

            // Verificar se o resultado da API indica falha (IResponseMessage.Success)
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
                        // Tentar extrair mensagem de erro
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
                    $"[MultiStepExecutor] Passo falhou. Mensagem da API: {apiMessage}"
                );
                return new
                {
                    success = false,
                    data = result,
                    message = apiMessage ?? "API retornou insucesso",
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
            Console.WriteLine($"[MultiStepExecutor] Transformando dados: {step.Transformation}");

            // Verificar se previousStepData é nulo ou inválido
            if (previousStepData == null)
            {
                Console.WriteLine("[MultiStepExecutor] ERRO: Dados do passo anterior são nulos.");
                return new
                {
                    success = false,
                    message = "Dados do passo anterior são nulos. Verifique se a etapa anterior foi executada corretamente.",
                    step = step.Description,
                };
            }

            var previousDataJson = JsonHelper.Serialize(previousStepData);
            var previousElement = JsonHelper.DeserializeToElement(previousDataJson);

            string dataToTransform = previousDataJson;
            
            // Se for um array, pegar o primeiro elemento para transformação
            if (previousElement.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine("[MultiStepExecutor] Dados do passo anterior são um array.");
                if (previousElement.GetArrayLength() > 0)
                {
                    var firstElement = previousElement[0];
                    dataToTransform = JsonHelper.Serialize(firstElement);
                    Console.WriteLine($"[MultiStepExecutor] Usando primeiro elemento do array para transformação.");
                }
                else
                {
                    Console.WriteLine("[MultiStepExecutor] ERRO: Array do passo anterior está vazio.");
                    return new
                    {
                        success = false,
                        message = "Array do passo anterior está vazio.",
                        step = step.Description,
                    };
                }
            }
            else if (previousElement.TryGetProperty("data", out JsonElement dataElement))
            {
                // Se 'data' for null dentro do objeto de resposta
                if (dataElement.ValueKind == JsonValueKind.Null)
                {
                    Console.WriteLine(
                        "[MultiStepExecutor] ERRO: Propriedade 'data' do passo anterior é nula."
                    );
                    return new
                    {
                        success = false,
                        message = "Nenhum dado retornado na etapa anterior para ser processado.",
                        step = step.Description,
                    };
                }
                
                // Se 'data' for um array, pegar o primeiro elemento
                if (dataElement.ValueKind == JsonValueKind.Array && dataElement.GetArrayLength() > 0)
                {
                    Console.WriteLine("[MultiStepExecutor] Propriedade 'data' é um array, usando primeiro elemento.");
                    dataToTransform = JsonHelper.Serialize(dataElement[0]);
                }
                else
                {
                    dataToTransform = JsonHelper.Serialize(dataElement);
                }
            }

            // Usar prompt melhorado que inclui a mensagem do usuário para verificação
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
                $"[MultiStepExecutor] Dados transformados (primeiros 500 chars): {transformedJson.Substring(0, Math.Min(500, transformedJson.Length))}"
            );

            // Verificar se o resultado é um array
            var transformedData = JsonHelper.DeserializeToElement(transformedJson);
            bool isArray = transformedData.ValueKind == JsonValueKind.Array;
            Console.WriteLine(
                $"[MultiStepExecutor] Tipo de resultado: {(isArray ? "Array" : "Object")}"
            );

            // Converter JsonElement para objetos CLR padrão (List/Dictionary) para garantir serialização correta
            var cleanData = JsonHelper.ConvertJsonElement(transformedData);

            // Se for array, retornar a lista
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

            // Validar que não contém RPs (apenas para objetos únicos)
            if (
                transformedData.TryGetProperty("rPs", out _)
                || transformedData.TryGetProperty("RPs", out _)
                || transformedData.TryGetProperty("solicitacaoCriacaoRps", out _)
            )
            {
                Console.WriteLine(
                    "[MultiStepExecutor] AVISO: JSON transformado contém RPs, removendo..."
                );

                transformedJson = JsonHelper.Serialize(cleanData);
                Console.WriteLine(
                    $"[MultiStepExecutor] JSON limpo (primeiros 500 chars): {transformedJson.Substring(0, Math.Min(500, transformedJson.Length))}"
                );
            }

            return new
            {
                success = true,
                data = cleanData,
                dataType = "PedidoNewModel",
                transformation = step.Transformation,
                step = step.Description,
            };
        }
    }
}
