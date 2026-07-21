using Doacoes.Api.Domain;
using MongoDB.Driver;

namespace Doacoes.Api.Infrastructure;

public class MongoDoacaoRepository : IDoacaoRepository
{
    public const string Colecao = "doacoes";
    private readonly IMongoCollection<Doacao> _doacoes;

    public MongoDoacaoRepository(IMongoDatabase database)
    {
        _doacoes = database.GetCollection<Doacao>(Colecao);
        // Índice para a consulta "minhas doações" (idempotente).
        _doacoes.Indexes.CreateOne(new CreateIndexModel<Doacao>(
            Builders<Doacao>.IndexKeys.Ascending(d => d.DoadorId)));
    }

    public Task AdicionarAsync(Doacao doacao, CancellationToken ct = default)
        => _doacoes.InsertOneAsync(doacao, cancellationToken: ct);

    public async Task<IReadOnlyList<Doacao>> ListarPorDoadorAsync(
        Guid doadorId, CancellationToken ct = default)
        => await _doacoes.Find(d => d.DoadorId == doadorId)
            .SortByDescending(d => d.CriadaEmUtc)
            .ToListAsync(ct);
}
