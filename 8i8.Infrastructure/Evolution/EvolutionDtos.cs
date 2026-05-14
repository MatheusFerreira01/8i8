using System.Text.Json.Serialization;

namespace ConnectAi.Infrastructure.Evolution;

public class EvolutionQrCodeResponse
{
    [JsonPropertyName("pairingCode")]
    public string? PairingCode { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("base64")]
    public string? Base64 { get; set; }
}

public class EvolutionCreateInstanceResponse
{
    [JsonPropertyName("instance")]
    public EvolutionInstanceInfo? Instance { get; set; }

    [JsonPropertyName("qrcode")]
    public EvolutionQrCodeResponse? Qrcode { get; set; }
}

public class EvolutionInstanceInfo
{
    [JsonPropertyName("instanceName")]
    public string InstanceName { get; set; } = default!;

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class EvolutionConnectionStateResponse
{
    [JsonPropertyName("instance")]
    public EvolutionConnectionInstance? Instance { get; set; }
}

public class EvolutionConnectionInstance
{
    [JsonPropertyName("instanceName")]
    public string InstanceName { get; set; } = default!;

    [JsonPropertyName("state")]
    public string State { get; set; } = default!;
}
