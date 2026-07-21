using Campanhas.Domain.Entities;

namespace Campanhas.Api.Services;

// SOLID (SRP/ISP): contrato pequeno e único — emitir tokens. Nada de login,
// senha ou persistência aqui.
public interface ITokenService
{
    (string AccessToken, DateTime ExpiraEmUtc) GerarToken(Usuario usuario);
}
