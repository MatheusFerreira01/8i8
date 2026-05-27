using System.Text.Json;
using ConnectAi.Api.Models.Evolution;
using ConnectAi.Infrastructure.Evolution;
using ConnectAi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ConnectAi.Api.Endpoints;

public static class WebhookEndpoints
{
    private static string CacheKey(string instance) => $"connection_state:{instance}";

    public static void MapWebhookEndpoints(this WebApplication app)
    {
        app.MapPost("/webhook/messages", async (
            HttpContext context,
            IServiceScopeFactory scopeFactory,
            IEvolutionApiClient evolution,
            IMemoryCache cache,
            ILoggerFactory loggerFactory,
            IConfiguration configuration) =>
        {
            var payload =
                await JsonSerializer.DeserializeAsync<EvolutionWebhookRequest>(
                    context.Request.Body);

            if (payload is null)
                return Results.Ok();

            if (payload.Event == "CONNECTION_UPDATE")
            {
                var state = payload.Data?.State;
                if (state is not null)
                    cache.Set(CacheKey(payload.Instance), state, TimeSpan.FromHours(1));

                return Results.Ok();
            }

            if (payload.Event != "messages.upsert")
                return Results.Ok();

            if (payload.Data.Key.FromMe)
                return Results.Ok();

            var isTextMessage = payload.Data.MessageType is "conversation" or "extendedTextMessage";
            if (!isTextMessage)
                return Results.Ok();

            var cachedState = cache.Get<string>(CacheKey(payload.Instance));
            if (cachedState is null)
            {
                cachedState = await evolution.GetConnectionStateAsync(payload.Instance);
                if (cachedState is not null)
                    cache.Set(CacheKey(payload.Instance), cachedState, TimeSpan.FromHours(1));
            }

            if (cachedState != "open")
                return Results.Ok();

            var userMessage = payload.Data.MessageType == "extendedTextMessage"
                ? payload.Data.Message.ExtendedTextMessage?.Text
                : payload.Data.Message.Conversation;

            if (string.IsNullOrWhiteSpace(userMessage))
                return Results.Ok();

            var number =
                payload.Data.Key.RemoteJid
                    .Replace("@s.whatsapp.net", "");

            var botEnabled = configuration.GetValue<bool>("ChatBot:Enabled", true);
            if (!botEnabled)
                return Results.Ok();

            var allowedNumbers = configuration.GetSection("ChatBot:AllowedNumbers").Get<List<string>>();
            if (allowedNumbers is { Count: > 0 } && !allowedNumbers.Contains(number))
                return Results.Ok();

            var instance = payload.Instance;
            var logger = loggerFactory.CreateLogger("Webhook");

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var categories = await db.Tags
                        .OrderBy(t => t.Name)
                        .Select(t => t.Name)
                        .ToListAsync();

                    var options = string.Join("\n", categories.Select((c, i) => $"{i + 1}. {c}"));

                    var message =
                        $"Olá! 👋 Sou o assistente virtual da [NOME DA EMPRESA].\n\n" +
                        $"Ainda estou em estado de desenvolvimento mas estou aqui para te auxiliar.\n\n" +
                        $"Como posso te ajudar? Escolha uma das categorias abaixo:\n\n" +
                        $"{options}\n\n" +
                        $"Digite o *número* da opção desejada.";

                    await evolution.SendTextMessageAsync(instance, number, message);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao processar mensagem de {Number}", number);
                }
            });

            return Results.Ok();
        });
    }
}
