using MongoDB.Bson;
using MongoDB.Driver;

namespace Doacoes.Worker.Persistence;

public class MongoStatusUpdater(IMongoDatabase database) : IStatusDoacaoAtualizador
{
    private readonly IMongoCollection<BsonDocument> _doacoes =
        database.GetCollection<BsonDocument>("doacoes");

    public Task MarcarProcessadaAsync(Guid doacaoId, DateTime quandoUtc, CancellationToken ct = default)
        => _doacoes.UpdateOneAsync(
            FiltroPorId(doacaoId),
            Builders<BsonDocument>.Update
                .Set("status", "Processada")
                .Set("processadaEmUtc", quandoUtc),
            cancellationToken: ct);

    public Task MarcarRejeitadaAsync(
        Guid doacaoId, string motivo, DateTime quandoUtc, CancellationToken ct = default)
        => _doacoes.UpdateOneAsync(
            FiltroPorId(doacaoId),
            Builders<BsonDocument>.Update
                .Set("status", "Rejeitada")
                .Set("processadaEmUtc", quandoUtc)
                .Set("motivoRejeicao", motivo),
            cancellationToken: ct);

    // Guid gravado pelo Doacoes.Api no formato Standard — o filtro usa a mesma
    // representação binária, sem depender de configuração global do driver.
    private static FilterDefinition<BsonDocument> FiltroPorId(Guid doacaoId)
        => Builders<BsonDocument>.Filter.Eq("_id",
            new BsonBinaryData(doacaoId, GuidRepresentation.Standard));
}
