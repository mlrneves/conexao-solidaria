using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace ConexaoSolidaria.Web.Services;

/// <summary>
/// AuthenticationStateProvider que lê o JWT do sessionStorage e materializa
/// as claims (sub/name/email/role) para o AuthorizeView/AuthorizeRouteView.
/// </summary>
public class JwtAuthenticationStateProvider(TokenStorage tokenStorage) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonimo =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStorage.ObterAsync();
        if (string.IsNullOrWhiteSpace(token))
            return Anonimo;

        var claims = LerClaims(token);
        if (claims is null)
        {
            await tokenStorage.RemoverAsync(); // token corrompido/expirado
            return Anonimo;
        }

        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task NotificarLoginAsync(string token)
    {
        await tokenStorage.GuardarAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task NotificarLogoutAsync()
    {
        await tokenStorage.RemoverAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>Decodifica o payload do JWT (base64url) sem validar assinatura —
    /// a validação real acontece nas APIs; aqui é só para a UI.</summary>
    private static List<Claim>? LerClaims(string token)
    {
        try
        {
            var partes = token.Split('.');
            if (partes.Length != 3) return null;

            var payload = partes[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var json = JsonDocument.Parse(Convert.FromBase64String(payload));

            // Token expirado não autentica.
            if (json.RootElement.TryGetProperty("exp", out var exp) &&
                DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) <= DateTimeOffset.UtcNow)
                return null;

            var claims = new List<Claim>();
            foreach (var prop in json.RootElement.EnumerateObject())
            {
                var tipo = prop.Name switch
                {
                    "sub" => ClaimTypes.NameIdentifier,
                    "name" => ClaimTypes.Name,
                    "email" => ClaimTypes.Email,
                    "role" => ClaimTypes.Role,
                    _ => prop.Name
                };
                claims.Add(new Claim(tipo, prop.Value.ToString()));
            }
            return claims;
        }
        catch
        {
            return null;
        }
    }
}
