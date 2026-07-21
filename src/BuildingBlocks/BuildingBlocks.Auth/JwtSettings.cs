namespace BuildingBlocks.Auth;

/// <summary>
/// Configuração JWT compartilhada entre os microsserviços (seção "Jwt").
/// A mesma chave simétrica (HS256) é usada pelo emissor (Campanhas.Api)
/// e pelos validadores (Doacoes.Api) — em produção viria de um cofre.
/// </summary>
public class JwtSettings
{
    public const string Secao = "Jwt";

    public string Issuer { get; set; } = "conexao-solidaria";
    public string Audience { get; set; } = "conexao-solidaria";
    public string Key { get; set; } = string.Empty;
    public int ExpiracaoMinutos { get; set; } = 120;
}
