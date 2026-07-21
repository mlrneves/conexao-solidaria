using BuildingBlocks.Messaging;
using Doacoes.Worker.Consumers;
using Doacoes.Worker.Persistence;
using Doacoes.Worker.Processing;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Prometheus;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ----- Postgres (banco de Campanhas — apenas incremento do total) -----
var connPostgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres ausente.");
builder.Services.AddDbContext<WorkerDbContext>(o => o.UseNpgsql(connPostgres));

// ----- MongoDB (status das doações) -----
var connMongo = builder.Configuration.GetConnectionString("Mongo")
    ?? throw new InvalidOperationException("ConnectionStrings:Mongo ausente.");
var mongoDatabase = builder.Configuration["Mongo:Database"] ?? "conexao_doacoes";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connMongo));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabase));
builder.Services.AddSingleton<IStatusDoacaoAtualizador, MongoStatusUpdater>();

builder.Services.AddScoped<IProcessamentoRepository, ProcessamentoRepository>();
builder.Services.AddScoped<IProcessadorDeDoacao, ProcessadorDeDoacao>();

// ----- RabbitMQ (consumer) -----
var rabbitSettings = builder.Configuration.GetSection(RabbitMqSettings.Secao).Get<RabbitMqSettings>()
    ?? new RabbitMqSettings();
builder.Services.AddSingleton(rabbitSettings);
var rabbitConnection = await RabbitMqConnector.ConectarComRetryAsync(
    rabbitSettings, msg => Console.WriteLine($"[bootstrap] {msg}"));
builder.Services.AddSingleton<IConnection>(rabbitConnection);
builder.Services.AddHostedService<DoacaoRecebidaConsumer>();

builder.Services.AddHealthChecks()
    .AddNpgSql(connPostgres, name: "postgres", tags: ["ready"])
    .AddMongoDb(name: "mongodb", tags: ["ready"])
    .AddRabbitMQ(name: "rabbitmq", tags: ["ready"]);

var app = builder.Build();

// Tabela de controle de idempotência (com retry — o Postgres pode subir depois).
{
    using var scope = app.Services.CreateScope();
    var repositorio = scope.ServiceProvider.GetRequiredService<IProcessamentoRepository>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    const int tentativas = 10;
    for (var i = 1; ; i++)
    {
        try
        {
            await repositorio.GarantirTabelaControleAsync();
            break;
        }
        catch (Exception ex) when (i < tentativas)
        {
            logger.LogWarning(
                "Postgres indisponível (tentativa {Tentativa}/{Total}): {Erro}. Aguardando 3s...",
                i, tentativas, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

app.MapGet("/", () => "Conexão Solidária — Doacoes.Worker (consumer RabbitMQ)");
app.MapMetrics(); // GET /metrics — inclui conexao_doacoes_processadas_total
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

app.Run();
