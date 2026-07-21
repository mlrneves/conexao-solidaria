using Campanhas.Domain.Enums;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.ValueObjects;

namespace Campanhas.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Cpf { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public PerfilUsuario Perfil { get; private set; }
    public bool ConsentimentoLgpd { get; private set; }
    public DateTime? DataConsentimentoUtc { get; private set; }
    public DateTime CriadoEmUtc { get; private set; }

    private Usuario() { } // EF Core

    /// <summary>
    /// Cadastro público de doador. Exige consentimento LGPD explícito e
    /// registra o momento do consentimento (accountability).
    /// </summary>
    public static Usuario CriarDoador(
        string nomeCompleto, string email, Cpf cpf, string senhaHash,
        bool consentimentoLgpd, DateTime agoraUtc)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new RegraDeNegocioException("Nome completo é obrigatório.");
        if (!consentimentoLgpd)
            throw new RegraDeNegocioException(
                "É necessário consentir com o tratamento dos dados pessoais (LGPD) para se cadastrar.");

        return new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = nomeCompleto.Trim(),
            Email = NormalizarEmail(email),
            Cpf = cpf.Valor,
            SenhaHash = senhaHash,
            Perfil = PerfilUsuario.Doador,
            ConsentimentoLgpd = true,
            DataConsentimentoUtc = agoraUtc,
            CriadoEmUtc = agoraUtc
        };
    }

    /// <summary>Usuário de gestão criado via seed (não há cadastro público de gestores).</summary>
    public static Usuario CriarGestor(string nomeCompleto, string email, string senhaHash, DateTime agoraUtc)
        => new()
        {
            Id = Guid.NewGuid(),
            NomeCompleto = nomeCompleto,
            Email = NormalizarEmail(email),
            Cpf = string.Empty,
            SenhaHash = senhaHash,
            Perfil = PerfilUsuario.GestorONG,
            ConsentimentoLgpd = false,
            CriadoEmUtc = agoraUtc
        };

    public static string NormalizarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new RegraDeNegocioException("E-mail é obrigatório.");
        return email.Trim().ToLowerInvariant();
    }
}
