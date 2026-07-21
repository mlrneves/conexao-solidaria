using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Infrastructure.Repositories;

// SOLID (LSP/DIP): implementação concreta do contrato do domínio; pode ser
// substituída por um fake em memória nos testes sem quebrar os consumidores.
public class CampanhaRepository(CampanhasDbContext db) : ICampanhaRepository
{
    public Task<Campanha?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Campanhas.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Campanha>> ListarTodasAsync(CancellationToken ct = default)
        => await db.Campanhas.AsNoTracking()
            .OrderByDescending(c => c.CriadoEmUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Campanha>> ListarPorStatusAsync(StatusCampanha status, CancellationToken ct = default)
        => await db.Campanhas.AsNoTracking()
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CriadoEmUtc)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Campanha campanha, CancellationToken ct = default)
        => await db.Campanhas.AddAsync(campanha, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
