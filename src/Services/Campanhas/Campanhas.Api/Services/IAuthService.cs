using Campanhas.Api.Dtos;

namespace Campanhas.Api.Services;

public interface IAuthService
{
    Task<UsuarioResponse> RegistrarDoadorAsync(RegistrarDoadorRequest request, CancellationToken ct);
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<MeResponse> ObterMeAsync(Guid usuarioId, CancellationToken ct);
}
