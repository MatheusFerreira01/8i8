using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace ConnectAi.Infrastructure.AI;

public class OllamaService : IOllamaService
{
    private readonly OllamaApiClient _client;
    private readonly OllamaSettings _settings;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(IOptions<OllamaSettings> settings, ILogger<OllamaService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new OllamaApiClient(new Uri(_settings.BaseUrl));
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var response = await _client.EmbedAsync(new EmbedRequest
        {
            Model = _settings.EmbedModel,
            Input = [text]
        }, ct);

        return response.Embeddings?[0] ?? [];
    }

    public async Task<string> GenerateChatResponseAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default)
    {
        var messages = new List<Message>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userMessage)
        };

        var request = new ChatRequest
        {
            Model = _settings.ChatModel,
            Messages = messages,
            Stream = false,
            Options = new OllamaSharp.Models.RequestOptions
            {
                NumPredict = _settings.NumPredict,
                NumCtx = _settings.NumCtx,
            }
        };

        try
        {
            ChatResponseStream? last = null;
            await foreach (var chunk in _client.ChatAsync(request, ct))
                last = chunk;

            return last?.Message?.Content ?? string.Empty;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"Modelo '{_settings.ChatModel}' não encontrado no Ollama. Execute: ollama pull {_settings.ChatModel}", ex);
        }
    }
}
