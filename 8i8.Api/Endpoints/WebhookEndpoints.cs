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
    private static string ConnectionKey(string instance) => $"connection_state:{instance}";
    private static string SendingKey(string number) => $"bot:sending:{number}";
    private static string LidMapKey(string jid) => $"lid_map:{jid}";

    private static string ExtractPhone(string? sender) =>
        (sender ?? "").Replace("@s.whatsapp.net", "").Replace("@c.us", "").Trim();

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
                    cache.Set(ConnectionKey(payload.Instance), state, TimeSpan.FromHours(1));

                return Results.Ok();
            }

            if (payload.Event != "messages.upsert")
                return Results.Ok();

            var isTextMessage = payload.Data.MessageType is "conversation" or "extendedTextMessage";
            if (!isTextMessage)
                return Results.Ok();

            var messageText = payload.Data.MessageType == "extendedTextMessage"
                ? payload.Data.Message.ExtendedTextMessage?.Text
                : payload.Data.Message.Conversation;

            if (string.IsNullOrWhiteSpace(messageText))
                return Results.Ok();

            var remoteJid = payload.Data.Key.RemoteJid;

            if (payload.Data.Key.FromMe)
            {
                if (cache.TryGetValue(LidMapKey(remoteJid), out string? clientPhone) && clientPhone is not null)
                {
                    if (cache.TryGetValue(SendingKey(clientPhone), out _))
                        return Results.Ok();

                    if (messageText.Trim().Equals("!bot on", StringComparison.OrdinalIgnoreCase))
                        cache.Remove(BotControlEndpoints.PausedCacheKey(clientPhone));
                    else
                        cache.Set(BotControlEndpoints.PausedCacheKey(clientPhone), true);
                }

                return Results.Ok();
            }

            var number = ExtractPhone(payload.Data.Key.SenderPn);

            if (string.IsNullOrEmpty(number))
                return Results.Ok();

            cache.Set(LidMapKey(remoteJid), number, TimeSpan.FromDays(1));

            var botEnabled = configuration.GetValue<bool>("ChatBot:Enabled", true);
            if (!botEnabled)
                return Results.Ok();

            var allowedNumbers = configuration.GetSection("ChatBot:AllowedNumbers").Get<List<string>>();
            if (allowedNumbers is { Count: > 0 } && !allowedNumbers.Contains(number))
                return Results.Ok();

            if (cache.IsPaused(number))
                return Results.Ok();

            var cachedState = cache.Get<string>(ConnectionKey(payload.Instance));
            if (cachedState is null)
            {
                cachedState = await evolution.GetConnectionStateAsync(payload.Instance);
                if (cachedState is not null)
                    cache.Set(ConnectionKey(payload.Instance), cachedState, TimeSpan.FromHours(1));
            }

            if (cachedState != "open")
                return Results.Ok();

            var instance = payload.Instance;
            var logger = loggerFactory.CreateLogger("Webhook");

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var tags = await db.Tags
                        .OrderBy(t => t.Name)
                        .ToListAsync();

                    string message;

                    if (int.TryParse(messageText.Trim(), out var selection)
                        && selection >= 1 && selection <= tags.Count)
                    {
                        var tag = tags[selection - 1];

                        var documents = await db.KnowledgeDocuments
                            .Where(d => d.IsActive && d.Tags.Any(t => t.TagId == tag.Id))
                            .OrderBy(d => d.Title)
                            .Select(d => new { d.Title, d.Content })
                            .ToListAsync();

                        if (documents.Count == 0)
                        {
                            message = $"📂 *{tag.Name}*\n\nAinda não temos informações cadastradas nessa categoria.\n\n" +
                                      $"Nossa IA está sendo implementada e em breve estará disponível para responder suas dúvidas automaticamente. 🚀";
                        }
                        else
                        {
                            var docs = string.Join("\n\n---\n\n", documents.Select(d => $"*{d.Title}*\n{d.Content}"));
                            message = $"📂 *{tag.Name}*\n\n{docs}\n\n---\n\n" +
                                      $"⚠️ _Nossa IA está sendo implementada e em breve responderá suas dúvidas de forma automática._";
                        }
                    }
                    else
                    {
                        var options = string.Join("\n", tags.Select((t, i) => $"{i + 1}. {t.Name}"));
                        message =
                            $"Olá! 👋 Sou o assistente virtual da 8i8.network.\n\n" +
                            $"Ainda estou em estado de desenvolvimento mas estou aqui para te auxiliar.\n\n" +
                            $"Como posso te ajudar? Escolha uma das categorias abaixo:\n\n" +
                            $"{options}\n\n" +
                            $"Digite o *número* da opção desejada.";
                    }

                    cache.Set(SendingKey(number), true, TimeSpan.FromSeconds(15));
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
