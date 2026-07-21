using System.ComponentModel.DataAnnotations;
using Campanhas.Domain.Enums;

namespace Campanhas.Api.Dtos;

public sealed class CriarCampanhaRequest
{
    [Required(ErrorMessage = "Título é obrigatório.")]
    [MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória.")]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    [Required]
    public decimal MetaFinanceira { get; set; }
}

public sealed class AtualizarCampanhaRequest
{
    [Required, MaxLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    public DateTime DataInicio { get; set; }

    [Required]
    public DateTime DataFim { get; set; }

    [Required]
    public decimal MetaFinanceira { get; set; }

    [Required]
    public StatusCampanha Status { get; set; }
}

/// <summary>Visão de gestão (todas as campanhas, qualquer status).</summary>
public sealed record CampanhaResponse(
    Guid Id, string Titulo, string Descricao, DateTime DataInicio, DateTime DataFim,
    decimal MetaFinanceira, StatusCampanha Status, decimal ValorTotalArrecadado,
    DateTime CriadoEmUtc);

/// <summary>
/// Painel de Transparência (público): título, meta e valor arrecadado,
/// conforme o edital — sem dados de pessoas.
/// </summary>
public sealed record CampanhaPublicaResponse(
    Guid Id, string Titulo, string Descricao, decimal MetaFinanceira,
    decimal ValorTotalArrecadado, DateTime DataFim);

/// <summary>Consulta pública mínima usada pelo serviço de Doações para validar o status.</summary>
public sealed record CampanhaStatusResponse(Guid Id, string Titulo, StatusCampanha Status);
