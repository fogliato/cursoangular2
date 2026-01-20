using System;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Service responsible for interacting with the Gemini API with retry logic
    /// </summary>
    public class GeminiApiService
    {
        public async Task<Microsoft.SemanticKernel.ChatMessageContent> ExecuteWithRetry(
            ChatHistory chat,
            IChatCompletionService chatService,
            int maxRetries = 3,
            int delaySeconds = 2
        )
        {
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine(
                        $"[GeminiApiService] Attempt {attempt}/{maxRetries} to call Gemini API..."
                    );

                    var result = await chatService.GetChatMessageContentAsync(chat);

                    Console.WriteLine(
                        $"[GeminiApiService] Successful call on attempt {attempt}"
                    );
                    return result;
                }
                catch (System.Net.Http.HttpRequestException ex)
                    when (ex.Message.Contains("503") || ex.Message.Contains("Service Unavailable"))
                {
                    lastException = ex;
                    Console.WriteLine(
                        $"[GeminiApiService] Error 503 on attempt {attempt}: Service temporarily unavailable"
                    );

                    if (attempt < maxRetries)
                    {
                        var delay = delaySeconds * attempt; // Exponential backoff
                        Console.WriteLine(
                            $"[GeminiApiService] Waiting {delay} seconds before retrying..."
                        );
                        await Task.Delay(delay * 1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[GeminiApiService] Error on attempt {attempt}: {ex.Message}"
                    );
                    throw;
                }
            }

            throw new Exception(
                $"Failed after {maxRetries} attempts. Last error: {lastException?.Message}",
                lastException
            );
        }
    }
}
