namespace ConnectAi.Infrastructure.Evolution;

public interface IEvolutionApiClient
{
    Task SendTextMessageAsync(string instance, string number, string text, CancellationToken ct = default);

    Task<EvolutionCreateInstanceResponse> CreateInstanceAsync(string instanceName, CancellationToken ct = default);

    Task<EvolutionCreateInstanceResponse> CreateInstanceForPhoneAsync(string instanceName, CancellationToken ct = default);

    Task<EvolutionQrCodeResponse?> GetQrCodeAsync(string instanceName, CancellationToken ct = default);

    Task<string?> GetConnectionStateAsync(string instanceName, CancellationToken ct = default);

    Task SetWebhookAsync(string instanceName, CancellationToken ct = default);

    Task<string?> GetPairingCodeAsync(string instanceName, string phoneNumber, CancellationToken ct = default);
}
