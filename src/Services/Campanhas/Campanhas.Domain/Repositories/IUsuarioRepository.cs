using Campanhas.Domain.Entities;

namespace Campanhas.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(string emailNormalizado, CancellationToken ct = default);
    Task<bool> EmailExisteAsync(string emailNormalizado, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
