using Doacoes.Api.Domain.Exceptions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Doacoes.Api.Domain;

public enum StatusDoacao
{
    Pendente,
    Processada,
    Rejeitada
}

/// <summary>
/// Intenção de doação persistida no MongoDB. Nasce Pendente; o Worker a marca
/// como Processada (total somado) ou Rejeitada (campanha não estava mais ativa).
/// </summary>
public class Doacao
{
    [BsonId]
    public Guid Id { get; private set; }

    public Guid CampanhaId { get; private set; }
    public Guid DoadorId { get; private set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Valor { get; private set; }

    [BsonRepresentation(BsonType.String)]
    public StatusDoacao Status { get; private set; }

    public DateTime CriadaEmUtc { get; private set; }
    public DateTime? ProcessadaEmUtc { get; private set; }
    public string? MotivoRejeicao { get; private set; }

    private Doacao() { } // driver

    public static Doacao Criar(Guid campanhaId, Guid doadorId, decimal valor, DateTime agoraUtc)
    {
        if (campanhaId == Guid.Empty)
            throw new DoacaoInvalidaException("Campanha é obrigatória.");
        if (valor <= 0)
            throw new DoacaoInvalidaException("O valor da doação deve ser maior que zero.");

        return new Doacao
        {
            Id = Guid.NewGuid(),
            CampanhaId = campanhaId,
            DoadorId = doadorId,
            Valor = valor,
            Status = StatusDoacao.Pendente,
            CriadaEmUtc = agoraUtc
        };
    }
}
