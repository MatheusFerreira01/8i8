using ConnectAi.Infrastructure.Evolution;

namespace ConnectAi.Api.Endpoints;

public static class InstanceEndpoints
{
    public static void MapInstanceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/instances");

        group.MapPost("/", async (CreateInstanceRequest body, IEvolutionApiClient evolution, CancellationToken ct) =>
        {
            var result = await evolution.CreateInstanceAsync(body.Name, ct);
            var instanceName = result.Instance?.InstanceName ?? body.Name;

            await evolution.SetWebhookAsync(instanceName, ct);

            var qrCode = result.Qrcode?.Base64 is not null
                ? result.Qrcode
                : await evolution.GetQrCodeAsync(instanceName, ct);

            if (qrCode?.Base64 is null)
            {
                var state = await evolution.GetConnectionStateAsync(instanceName, ct);
                return Results.Ok(new
                {
                    instanceName,
                    state = state ?? "unknown",
                    connected = state == "open",
                    qrCode = (string?)null
                });
            }

            return Results.Ok(new
            {
                instanceName,
                state = "connecting",
                connected = false,
                qrCode = qrCode.Base64
            });
        });

        group.MapPost("/connect-phone", async (ConnectPhoneRequest body, IEvolutionApiClient evolution, CancellationToken ct) =>
        {
            var result = await evolution.CreateInstanceForPhoneAsync(body.Name, ct);
            var instanceName = result.Instance?.InstanceName ?? body.Name;

            await evolution.SetWebhookAsync(instanceName, ct);

            var pairingCode = await evolution.GetPairingCodeAsync(instanceName, body.PhoneNumber, ct);

            if (pairingCode is null)
                return Results.BadRequest(new { error = "Não foi possível gerar o código de pareamento. Verifique o número informado." });

            return Results.Ok(new
            {
                instanceName,
                pairingCode,
                instruction = "Abra o WhatsApp > Dispositivos conectados > Conectar dispositivo > Conectar com número de telefone. Digite o código acima."
            });
        });

        group.MapGet("/{name}/qrcode", async (string name, IEvolutionApiClient evolution, CancellationToken ct) =>
        {
            var state = await evolution.GetConnectionStateAsync(name, ct);

            if (state is null)
                return Results.NotFound(new { error = $"Instância '{name}' não encontrada." });

            if (state == "open")
                return Results.Ok(new { instanceName = name, state, connected = true, qrCode = (string?)null });

            var qrCode = await evolution.GetQrCodeAsync(name, ct);

            if (qrCode?.Base64 is null)
                return Results.Ok(new { instanceName = name, state, connected = false, qrCode = (string?)null });

            return Results.Ok(new
            {
                instanceName = name,
                state,
                connected = false,
                qrCode = qrCode.Base64
            });
        });

        group.MapGet("/{name}/status", async (string name, IEvolutionApiClient evolution, CancellationToken ct) =>
        {
            var state = await evolution.GetConnectionStateAsync(name, ct);

            if (state is null)
                return Results.NotFound(new { error = $"Instância '{name}' não encontrada." });

            return Results.Ok(new
            {
                instanceName = name,
                state,
                connected = state == "open"
            });
        });
    }
}

public record CreateInstanceRequest(string Name);
public record ConnectPhoneRequest(string Name, string PhoneNumber);
