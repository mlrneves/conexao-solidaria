using Doacoes.Api.Domain;

namespace Doacoes.Api.Infrastructure;

// SOLID (ISP): contrato mínimo — o serviço de doações só precisa gravar e
// listar por doador. Atualização de status é responsabilidade do Worker.
public interface IDoacaoRepository
{
    Task AdicionarAsync(Doacao doacao, CancellationToken ct = default);
    Task<IReadOnlyList<Doacao>> ListarPorDoadorAsync(Guid doadorId, CancellationToken ct = default);
}
