using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;

namespace Campanhas.Domain.Repositories;

// SOLID (DIP): o domínio define o contrato; a infraestrutura (EF/Postgres)
// implementa. Serviços de aplicação dependem apenas desta abstração.
public interface ICampanhaRepository
{
    Task<Campanha?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Campanha>> ListarTodasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Campanha>> ListarPorStatusAsync(StatusCampanha status, CancellationToken ct = default);
    Task AdicionarAsync(Campanha campanha, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
