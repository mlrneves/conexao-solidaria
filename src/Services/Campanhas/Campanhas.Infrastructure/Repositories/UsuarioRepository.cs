using Campanhas.Domain.Entities;
using Campanhas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Infrastructure.Repositories;

public class UsuarioRepository(CampanhasDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Usuario?> ObterPorEmailAsync(string emailNormalizado, CancellationToken ct = default)
        => db.Usuarios.FirstOrDefaultAsync(u => u.Email == emailNormalizado, ct);

    public Task<bool> EmailExisteAsync(string emailNormalizado, CancellationToken ct = default)
        => db.Usuarios.AnyAsync(u => u.Email == emailNormalizado, ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
        => await db.Usuarios.AddAsync(usuario, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
