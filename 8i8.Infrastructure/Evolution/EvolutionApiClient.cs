using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace ConnectAi.Infrastructure.Evolution;

public class EvolutionApiClient : IEvolutionApiClient
{
    private readonly HttpClient _http;
    private readonly EvolutionSettings _settings;

    public EvolutionApiClient(HttpClient http, IOptions<EvolutionSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
    }

    public async Task SendTextMessageAsync(string instance, string number, string text, CancellationToken ct = default)
    {
        var response = await SendAsync(HttpMethod.Post, $"/message/sendText/{instance}", new { number, text }, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<EvolutionCreateInstanceResponse> CreateInstanceAsync(string instanceName, CancellationToken ct = default)
    {
        var body = new { instanceName, qrcode = true, integration = "WHATSAPP-BAILEYS" };
        var response = await SendAsync(HttpMethod.Post, "/instance/create", body, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EvolutionCreateInstanceResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Resposta inválida ao criar instância.");
    }

    public async Task<EvolutionCreateInstanceResponse> CreateInstanceForPhoneAsync(string instanceName, CancellationToken ct = default)
    {
        var body = new { instanceName, qrcode = false, integration = "WHATSAPP-BAILEYS" };
        var response = await SendAsync(HttpMethod.Post, "/instance/create", body, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            return new EvolutionCreateInstanceResponse { Instance = new EvolutionInstanceInfo { InstanceName = instanceName } };

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EvolutionCreateInstanceResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Resposta inválida ao criar instância.");
    }

    public async Task<EvolutionQrCodeResponse?> GetQrCodeAsync(string instanceName, CancellationToken ct = default)
    {
        var response = await SendAsync(HttpMethod.Get, $"/instance/connect/{instanceName}", body: null, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EvolutionQrCodeResponse>(cancellationToken: ct);
        return result?.Base64 is not null ? result : null;
    }

    public async Task<string?> GetConnectionStateAsync(string instanceName, CancellationToken ct = default)
    {
        var response = await SendAsync(HttpMethod.Get, $"/instance/connectionState/{instanceName}", body: null, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<EvolutionConnectionStateResponse>(cancellationToken: ct);
        return result?.Instance?.State;
    }

    public async Task<string?> GetPairingCodeAsync(string instanceName, string phoneNumber, CancellationToken ct = default)
    {
        var response = await SendAsync(HttpMethod.Get, $"/instance/connect/{instanceName}?number={phoneNumber}", body: null, ct);

        if (!response.IsSuccessStatusCode)
            return null;

        var raw = await response.Content.ReadAsStringAsync(ct);
        Console.WriteLine($"[DBG] {raw}");
        var result = System.Text.Json.JsonSerializer.Deserialize<EvolutionQrCodeResponse>(raw);
        return result?.PairingCode ?? result?.Code;
    }

    public async Task SetWebhookAsync(string instanceName, CancellationToken ct = default)
    {
        var body = new
        {
            instanceName,
            token = _settings.ApiKey,
            qrcode = true,
            integration = "WHATSAPP-BAILEYS",
            webhook = new
            {
                enabled = true,
                url = _settings.WebhookUrl,
                byEvents = false,
                base64 = false,
                events = new[] { "MESSAGES_UPSERT", "CONNECTION_UPDATE", "QRCODE_UPDATED" }
            }
        };

        var response = await SendAsync(HttpMethod.Post, $"/webhook/set/{instanceName}", body, ct);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, $"{_settings.BaseUrl}{path}");
        request.Headers.Add("apikey", _settings.ApiKey);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return await _http.SendAsync(request, ct);
    }
}
