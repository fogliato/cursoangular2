using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ProAgil.Domain.Agent
{
    public interface IAgentService
    {
        Task<object> ProcessMessage(string message);
    }
    /// <summary>
    /// Serviço principal que coordena o processamento de mensagens usando IA
    /// para identificar e executar ações em controllers
    /// </summary>
    public class AgentService : IAgentService
    {
        private readonly IChatCompletionService _chatService;
        private readonly ApiDocumentationService _apiDocumentationService;
        private readonly ParameterExtractionService _parameterExtraction;
        private readonly ControllerInvoker _controllerInvoker;
        private readonly MultiStepExecutor _multiStepExecutor;
        private readonly GeminiApiService _geminiService;

        public AgentService(
            IServiceProvider serviceProvider,
            IApiDescriptionGroupCollectionProvider apiExplorer
        )
        {
            var geminiApiKey = EnvironmentVariableExtensions.GetEnvironmentVariable<string>(
                "GEMINI_API_KEY",
                "GEMINI_API_KEY"
            );

            var geminiModel = EnvironmentVariableExtensions.GetEnvironmentVariable<string>(
                "GEMINI_MODEL",
                "GEMINI_MODEL"
            );

            if (string.IsNullOrEmpty(geminiModel))
            {
                geminiModel = "gemini-2.5-flash";
            }

            Console.WriteLine($"[AgentService] Inicializando com modelo: {geminiModel}");
            Console.WriteLine(
                $"[AgentService] API Key configurada: {!string.IsNullOrEmpty(geminiApiKey)}"
            );

            var builder = Kernel.CreateBuilder();
            builder.AddGoogleAIGeminiChatCompletion(geminiModel, geminiApiKey ?? string.Empty);

            var kernel = builder.Build();
            _chatService = kernel.GetRequiredService<IChatCompletionService>();

            // Inicializar serviços especializados
            _apiDocumentationService = new ApiDocumentationService(apiExplorer);
            _geminiService = new GeminiApiService();
            _parameterExtraction = new ParameterExtractionService();
            _controllerInvoker = new ControllerInvoker(serviceProvider);
            _multiStepExecutor = new MultiStepExecutor(
                _controllerInvoker,
                _parameterExtraction,
                _geminiService,
                _chatService
            );
        }

        public async Task<object> ProcessMessage(string message)
        {
            try
            {
                Console.WriteLine($"[AgentService] Processando mensagem: {message}");

                // Gerar documentação da API
                var apiDocumentation = _apiDocumentationService.GenerateApiDocumentation();

                // Verificar se é uma operação multi-etapa
                var executionPlan = await _multiStepExecutor.AnalyzeExecutionPlan(message);

                if (executionPlan.IsMultiStep)
                {
                    Console.WriteLine(
                        $"[AgentService] Detectada operação multi-etapa com {executionPlan.Steps.Count} passos"
                    );
                    return await _multiStepExecutor.ExecuteMultiStepOperation(executionPlan);
                }

                // Operação simples - executar em uma única etapa
                return await ExecuteSingleOperation(message, apiDocumentation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AgentService] Erro ao processar mensagem: {ex.Message}");
                return new
                {
                    success = false,
                    message = $"Erro ao processar: {ex.Message}",
                    stackTrace = ex.StackTrace,
                };
            }
        }

        private async Task<object> ExecuteSingleOperation(string message, string apiDocumentation)
        {
            // Etapa 1: Identificar controller e action
            Console.WriteLine($"[AgentService] Identificando controller e action...");
            var identificationPrompt = AgentPrompts.BuildIdentificationPrompt(
                apiDocumentation,
                message
            );

            var chat = new ChatHistory();
            chat.AddUserMessage(identificationPrompt);

            var identificationResult = await _geminiService.ExecuteWithRetry(chat, _chatService);
            var controllerActionString = identificationResult.Content?.Trim();

            if (string.IsNullOrEmpty(controllerActionString))
            {
                return new
                {
                    success = false,
                    message = "Não foi possível identificar a ação a executar.",
                };
            }

            var parts = controllerActionString.Split('.');
            if (parts.Length != 2)
            {
                return new
                {
                    success = false,
                    message = $"Formato inválido: {controllerActionString}",
                };
            }

            var controllerName = parts[0];
            var actionName = parts[1];

            // Etapa 2: Encontrar o método
            var (_, method) = _controllerInvoker.FindControllerAndMethod(
                controllerName,
                actionName
            );

            // Etapa 3: Extrair parâmetros
            var parameters = method.GetParameters();
            var methodParams = await _parameterExtraction.ExtractMethodParameters(
                message,
                controllerName,
                actionName,
                parameters,
                _chatService
            );

            // Etapa 4: Executar o método
            var result = await _controllerInvoker.InvokeController(
                controllerName,
                actionName,
                methodParams
            );

            // Etapa 5: Retornar resultado
            return new
            {
                success = true,
                data = result,
                controller = controllerName,
                action = actionName,
            };
        }
    }
}
