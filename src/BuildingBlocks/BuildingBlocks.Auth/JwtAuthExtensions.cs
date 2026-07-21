using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Auth;

// SOLID (DIP/SRP): os serviços não conhecem detalhes de validação de token —
// apenas chamam AddConexaoJwt(). A config compartilhada garante que o token
// emitido pelo Campanhas.Api seja aceito pelo Doacoes.Api.
public static class JwtAuthExtensions
{
    public static IServiceCollection AddConexaoJwt(
        this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtSettings.Secao).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Seção de configuração 'Jwt' ausente.");

        if (string.IsNullOrWhiteSpace(settings.Key) || settings.Key.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Key precisa ter pelo menos 32 caracteres (256 bits) para HS256.");

        services.AddSingleton(settings);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
            });

        services.AddAuthorization();
        return services;
    }
}
