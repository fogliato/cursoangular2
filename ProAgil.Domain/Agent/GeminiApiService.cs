using System;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Serviço responsável por interagir com a API Gemini com retry logic
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
                        $"[GeminiApiService] Tentativa {attempt}/{maxRetries} de chamada à API Gemini..."
                    );

                    var result = await chatService.GetChatMessageContentAsync(chat);

                    Console.WriteLine(
                        $"[GeminiApiService] Chamada bem-sucedida na tentativa {attempt}"
                    );
                    return result;
                }
                catch (System.Net.Http.HttpRequestException ex)
                    when (ex.Message.Contains("503") || ex.Message.Contains("Service Unavailable"))
                {
                    lastException = ex;
                    Console.WriteLine(
                        $"[GeminiApiService] Erro 503 na tentativa {attempt}: Serviço temporariamente indisponível"
                    );

                    if (attempt < maxRetries)
                    {
                        var delay = delaySeconds * attempt; // Backoff exponencial
                        Console.WriteLine(
                            $"[GeminiApiService] Aguardando {delay} segundos antes de tentar novamente..."
                        );
                        await Task.Delay(delay * 1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[GeminiApiService] Erro na tentativa {attempt}: {ex.Message}"
                    );
                    throw;
                }
            }

            throw new Exception(
                $"Falha após {maxRetries} tentativas. Último erro: {lastException?.Message}",
                lastException
            );
        }
    }
}
