using ConnectAi.Api.Endpoints;
using ConnectAi.Infrastructure.AI;
using ConnectAi.Infrastructure.Evolution;
using ConnectAi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console();
});

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var postgresConnection = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' não configurada. " +
        "Defina ConnectionStrings__Postgres (via .env / docker-compose) " +
        "ou no appsettings.Development.json.");

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(postgresConnection, npgsql => npgsql.UseVector()));

builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("Ollama"));
builder.Services.AddSingleton<IOllamaService, OllamaService>();

builder.Services.Configure<EvolutionSettings>(builder.Configuration.GetSection("EvolutionApi"));
builder.Services.AddHttpClient<IEvolutionApiClient, EvolutionApiClient>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

app.UseSerilogRequestLogging();
app.UseCors("Default");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    Application = "8i8 API",
    Environment = app.Environment.EnvironmentName,
    Status = "Running"
}));

app.MapInstanceEndpoints();
app.MapWebhookEndpoints();

app.Run();
