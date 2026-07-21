using System.ComponentModel.DataAnnotations;
using Doacoes.Api.Domain;

namespace Doacoes.Api.Dtos;

public sealed class DoarRequest
{
    [Required]
    public Guid CampanhaId { get; set; }

    [Required]
    public decimal Valor { get; set; }
}

/// <summary>
/// Resposta 202 Accepted: a doação foi ACEITA e será processada de forma
/// assíncrona pelo Worker via RabbitMQ — a API não soma o total diretamente.
/// </summary>
public sealed record DoacaoAceitaResponse(Guid DoacaoId, StatusDoacao Status, string Mensagem);

public sealed record MinhaDoacaoResponse(
    Guid Id, Guid CampanhaId, decimal Valor, StatusDoacao Status,
    DateTime CriadaEmUtc, DateTime? ProcessadaEmUtc, string? MotivoRejeicao);
