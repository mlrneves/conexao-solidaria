using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuildingBlocks.Auth;
using Campanhas.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Campanhas.Api.Services;

public class TokenService(JwtSettings settings) : ITokenService
{
    public (string AccessToken, DateTime ExpiraEmUtc) GerarToken(Usuario usuario)
    {
        var expiraEmUtc = DateTime.UtcNow.AddMinutes(settings.ExpiracaoMinutos);

        // Claims com nomes curtos (sub/name/email/role): legíveis no jwt.io e
        // remapeadas automaticamente pelo JwtBearer no lado validador.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Name, usuario.NomeCompleto),
            new("role", usuario.Perfil.ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEmUtc,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEmUtc);
    }
}
