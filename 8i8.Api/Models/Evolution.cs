using System.Text.Json.Serialization;

namespace ConnectAi.Api.Models.Evolution;

public class EvolutionWebhookRequest
{
    [JsonPropertyName("event")]
    public string Event { get; set; } = default!;
    [JsonPropertyName("instance")]
    public string Instance { get; set; } = default!;
    [JsonPropertyName("data")]

    public EvolutionMessageData Data { get; set; } = default!;
}

public class EvolutionMessageData
{
    [JsonPropertyName("key")]
    public EvolutionKey Key { get; set; } = default!;

    [JsonPropertyName("message")]
    public EvolutionMessage Message { get; set; } = default!;

    [JsonPropertyName("messageType")]
    public string MessageType { get; set; } = default!;

    [JsonPropertyName("state")]
    public string? State { get; set; }
}

public class EvolutionKey
{
    [JsonPropertyName("remoteJid")]    
    public string RemoteJid { get; set; } = default!;
    [JsonPropertyName("fromMe")]
    public bool FromMe { get; set; }
}

public class EvolutionMessage
{
    [JsonPropertyName("conversation")]
    public string? Conversation { get; set; }

    [JsonPropertyName("extendedTextMessage")]
    public EvolutionExtendedTextMessage? ExtendedTextMessage { get; set; }
}

public class EvolutionExtendedTextMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}