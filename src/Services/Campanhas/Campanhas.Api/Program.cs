using System.Text.Json.Serialization;
using BuildingBlocks.Auth;
using Campanhas.Api.Infrastructure;
using Campanhas.Api.Services;
using Campanhas.Domain.Repositories;
using Campanhas.Infrastructure;
using Campanhas.Infrastructure.Repositories;
using Campanhas.Infrastructure.Seed;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var connPostgres = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres ausente.");

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<CampanhasDbContext>(o =>
    o.UseNpgsql(connPostgres, npgsql => npgsql.EnableRetryOnFailure(5)));

// SOLID (DIP): raiz de composição — único lugar que conhece as implementações concretas.
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICampanhaRepository, CampanhaRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICampanhaService, CampanhaService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddConexaoJwt(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DominioExceptionHandler>();

var origensPermitidas = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddPolicy("web", p =>
    p.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Conexão Solidária — Campanhas.Api",
        Version = "v1",
        Description = "Autenticação (JWT/RBAC), usuários, gestão de campanhas e Painel de Transparência."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole o token JWT obtido em /api/auth/login."
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

// Observabilidade: /health (agregado), /health/ready (dependências), /health/live (self).
builder.Services.AddHealthChecks()
    .AddNpgSql(connPostgres, name: "postgres", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();

// Swagger habilitado em todos os ambientes: é a interface de demonstração do MVP.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Campanhas.Api v1"));

app.UseCors("web");

app.UseHttpMetrics(); // contadores/histogramas HTTP para o Prometheus

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics(); // GET /metrics
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// Migrations + seed do GestorONG no startup (idempotente; replicas:1 no K8s).
if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CampanhasDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.MigrarESemearAsync(
        db,
        app.Configuration["Seed:GestorEmail"] ?? "gestor@conexaosolidaria.org",
        app.Configuration["Seed:GestorPassword"] ?? "Gestor@123!",
        logger);
}

app.Run();
