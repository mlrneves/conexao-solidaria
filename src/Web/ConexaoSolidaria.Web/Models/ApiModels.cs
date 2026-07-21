using System.ComponentModel.DataAnnotations;

namespace ConexaoSolidaria.Web.Models;

// ----- Respostas das APIs (status/perfil como string: JSON usa enum-string) -----

public record CampanhaPublica(
    Guid Id, string Titulo, string Descricao, decimal MetaFinanceira,
    decimal ValorTotalArrecadado, DateTime DataFim)
{
    public double PercentualMeta =>
        MetaFinanceira <= 0 ? 0 : Math.Min(100, (double)(ValorTotalArrecadado / MetaFinanceira * 100));
}

public record CampanhaGestor(
    Guid Id, string Titulo, string Descricao, DateTime DataInicio, DateTime DataFim,
    decimal MetaFinanceira, string Status, decimal ValorTotalArrecadado, DateTime CriadoEmUtc);

public record LoginResult(string AccessToken, DateTime ExpiraEmUtc, string NomeCompleto, string Perfil);

public record Me(Guid Id, string NomeCompleto, string Email, string Perfil, string? CpfMascarado);

public record DoacaoAceita(Guid DoacaoId, string Status, string Mensagem);

public record MinhaDoacao(
    Guid Id, Guid CampanhaId, decimal Valor, string Status,
    DateTime CriadaEmUtc, DateTime? ProcessadaEmUtc, string? MotivoRejeicao);

// ----- Formulários (validação client-side com DataAnnotations) -----

public class LoginForm
{
    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    public string Senha { get; set; } = string.Empty;
}

public class CadastroForm
{
    [Required(ErrorMessage = "Informe o nome completo."), MaxLength(200)]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha."), MinLength(8, ErrorMessage = "Mínimo de 8 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true",
        ErrorMessage = "É necessário consentir com o tratamento dos dados (LGPD).")]
    public bool ConsentimentoLgpd { get; set; }
}

public class CampanhaForm
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o título."), MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descrição.")]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public DateTime DataInicio { get; set; } = DateTime.Today;

    [Required]
    public DateTime DataFim { get; set; } = DateTime.Today.AddDays(30);

    [Required, Range(0.01, 999_999_999, ErrorMessage = "A meta deve ser maior que zero.")]
    public decimal MetaFinanceira { get; set; }

    public string Status { get; set; } = "Ativa";
}

public class DoacaoForm
{
    [Required, Range(0.01, 999_999_999, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Valor { get; set; } = 50m;
}
