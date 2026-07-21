using System.ComponentModel.DataAnnotations;
using Campanhas.Domain.Enums;

namespace Campanhas.Api.Dtos;

public sealed class RegistrarDoadorRequest
{
    [Required(ErrorMessage = "Nome completo é obrigatório.")]
    [MaxLength(200)]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "Senha deve ter pelo menos 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    /// <summary>Consentimento LGPD explícito para tratamento dos dados pessoais.</summary>
    public bool ConsentimentoLgpd { get; set; }
}

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Resposta de cadastro — nunca expõe CPF nem senha (minimização LGPD).</summary>
public sealed record UsuarioResponse(Guid Id, string NomeCompleto, string Email, PerfilUsuario Perfil);

public sealed record LoginResponse(
    string AccessToken, DateTime ExpiraEmUtc, string NomeCompleto, PerfilUsuario Perfil);

/// <summary>Dados do próprio titular — CPF apenas mascarado (LGPD).</summary>
public sealed record MeResponse(
    Guid Id, string NomeCompleto, string Email, PerfilUsuario Perfil, string? CpfMascarado);
