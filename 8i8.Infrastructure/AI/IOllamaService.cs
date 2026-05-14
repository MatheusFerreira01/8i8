namespace ConnectAi.Infrastructure.AI;

public interface IOllamaService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);

    Task<string> GenerateChatResponseAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default);
}
