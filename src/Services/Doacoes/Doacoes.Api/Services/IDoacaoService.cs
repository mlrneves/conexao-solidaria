using Doacoes.Api.Dtos;

namespace Doacoes.Api.Services;

public interface IDoacaoService
{
    Task<DoacaoAceitaResponse> DoarAsync(DoarRequest request, Guid doadorId, CancellationToken ct);
    Task<IReadOnlyList<MinhaDoacaoResponse>> ListarMinhasAsync(Guid doadorId, CancellationToken ct);
}
