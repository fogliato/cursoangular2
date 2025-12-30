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
    /// Main service that coordinates message processing using AI
    /// to identify and execute actions on controllers
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

            Console.WriteLine($"[AgentService] Initializing with model: {geminiModel}");
            Console.WriteLine(
                $"[AgentService] API Key configured: {!string.IsNullOrEmpty(geminiApiKey)}"
            );

            var builder = Kernel.CreateBuilder();
            builder.AddGoogleAIGeminiChatCompletion(geminiModel, geminiApiKey ?? string.Empty);

            var kernel = builder.Build();
            _chatService = kernel.GetRequiredService<IChatCompletionService>();

            // Initialize specialized services
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
                Console.WriteLine($"[AgentService] Processing message: {message}");

                // Generate API documentation
                var apiDocumentation = _apiDocumentationService.GenerateApiDocumentation();

                // Check if it's a multi-step operation
                var executionPlan = await _multiStepExecutor.AnalyzeExecutionPlan(message);

                if (executionPlan.IsMultiStep)
                {
                    Console.WriteLine(
                        $"[AgentService] Multi-step operation detected with {executionPlan.Steps.Count} steps"
                    );
                    return await _multiStepExecutor.ExecuteMultiStepOperation(executionPlan);
                }

                // Simple operation - execute in a single step
                return await ExecuteSingleOperation(message, apiDocumentation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AgentService] Error processing message: {ex.Message}");
                return new
                {
                    success = false,
                    message = $"Error processing: {ex.Message}",
                    stackTrace = ex.StackTrace,
                };
            }
        }

        private async Task<object> ExecuteSingleOperation(string message, string apiDocumentation)
        {
            // Step 1: Identify controller and action
            Console.WriteLine($"[AgentService] Identifying controller and action...");
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
                    message = "Could not identify the action to execute.",
                };
            }

            var parts = controllerActionString.Split('.');
            if (parts.Length != 2)
            {
                return new
                {
                    success = false,
                    message = $"Invalid format: {controllerActionString}",
                };
            }

            var controllerName = parts[0];
            var actionName = parts[1];

            // Step 2: Find the method
            var (_, method) = _controllerInvoker.FindControllerAndMethod(
                controllerName,
                actionName
            );

            // Step 3: Extract parameters
            var parameters = method.GetParameters();
            var methodParams = await _parameterExtraction.ExtractMethodParameters(
                message,
                controllerName,
                actionName,
                parameters,
                _chatService
            );

            // Step 4: Execute the method
            var result = await _controllerInvoker.InvokeController(
                controllerName,
                actionName,
                methodParams
            );

            // Step 5: Return result
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
