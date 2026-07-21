using Campanhas.Api.Dtos;

namespace Campanhas.Api.Services;

public interface ICampanhaService
{
    Task<CampanhaResponse> CriarAsync(CriarCampanhaRequest request, Guid gestorId, CancellationToken ct);
    Task<CampanhaResponse> AtualizarAsync(Guid id, AtualizarCampanhaRequest request, CancellationToken ct);
    Task<IReadOnlyList<CampanhaResponse>> ListarTodasAsync(CancellationToken ct);
    Task<CampanhaResponse> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<CampanhaPublicaResponse>> ListarPainelPublicoAsync(CancellationToken ct);
    Task<CampanhaStatusResponse> ObterStatusPublicoAsync(Guid id, CancellationToken ct);
}
