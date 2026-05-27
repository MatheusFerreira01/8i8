using Microsoft.Extensions.Caching.Memory;

namespace ConnectAi.Api.Endpoints;

public static class BotControlEndpoints
{
    public static string PausedCacheKey(string number) => $"bot:paused:{number}";
    private static string PausedKey(string number) => PausedCacheKey(number);

    public static void MapBotControlEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/bot");

        group.MapPost("/pause/{number}", (string number, IMemoryCache cache) =>
        {
            cache.Set(PausedKey(number), true);
            return Results.Ok(new { number, status = "paused" });
        });

        group.MapPost("/resume/{number}", (string number, IMemoryCache cache) =>
        {
            cache.Remove(PausedKey(number));
            return Results.Ok(new { number, status = "active" });
        });

        group.MapGet("/status/{number}", (string number, IMemoryCache cache) =>
        {
            var paused = cache.TryGetValue(PausedKey(number), out _);
            return Results.Ok(new { number, status = paused ? "paused" : "active" });
        });
    }

    public static bool IsPaused(this IMemoryCache cache, string number) =>
        cache.TryGetValue(PausedKey(number), out _);
}
