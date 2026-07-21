using System.Text.Json.Serialization;
using BuildingBlocks.Auth;
using BuildingBlocks.Messaging;
using Doacoes.Api.Infrastructure;
using Doacoes.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Prometheus;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ----- MongoDB (banco de doações) -----
MongoConfig.Registrar();
var connMongo = builder.Configuration.GetConnectionString("Mongo")
    ?? throw new InvalidOperationException("ConnectionStrings:Mongo ausente.");
var mongoDatabase = builder.Configuration["Mongo:Database"] ?? "conexao_doacoes";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(connMongo));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabase));
builder.Services.AddSingleton<IDoacaoRepository, MongoDoacaoRepository>();

// ----- RabbitMQ (publisher do DoacaoRecebidaEvent) -----
var rabbitSettings = builder.Configuration.GetSection(RabbitMqSettings.Secao).Get<RabbitMqSettings>()
    ?? new RabbitMqSettings();
builder.Services.AddSingleton(rabbitSettings);
var rabbitConnection = await RabbitMqConnector.ConectarComRetryAsync(
    rabbitSettings, msg => Console.WriteLine($"[bootstrap] {msg}"));
builder.Services.AddSingleton<IConnection>(rabbitConnection);
builder.Services.AddSingleton<IEventBus, RabbitMqEventBus>();

// ----- Client HTTP resiliente para o Campanhas.Api -----
var campanhasBaseUrl = builder.Configuration["Campanhas:BaseUrl"]
    ?? throw new InvalidOperationException("Campanhas:BaseUrl ausente.");
builder.Services.AddHttpClient<ICampanhasClient, CampanhasHttpClient>(c =>
        c.BaseAddress = new Uri(campanhasBaseUrl))
    .AddStandardResilienceHandler(); // retry + circuit breaker + timeout

builder.Services.AddScoped<IDoacaoService, DoacaoService>();

builder.Services.AddConexaoJwt(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DoacoesExceptionHandler>();

var origensPermitidas = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddPolicy("web", p =>
    p.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conexão Solidária — Doacoes.Api",
        Version = "v1",
        Description = "Recebe intenções de doação e publica DoacaoRecebidaEvent no RabbitMQ (processamento assíncrono)."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole o token JWT obtido no Campanhas.Api (/api/auth/login)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
});

builder.Services.AddHealthChecks()
    .AddMongoDb(name: "mongodb", tags: ["ready"])
    .AddRabbitMQ(name: "rabbitmq", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Doacoes.Api v1"));

app.UseCors("web");

app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

app.Run();
