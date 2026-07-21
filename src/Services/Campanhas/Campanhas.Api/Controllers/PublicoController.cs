using Campanhas.Api.Dtos;
using Campanhas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Controllers;

/// <summary>
/// Painel de Transparência — API pública (requisito 4 do edital):
/// somente campanhas Ativas, com título, meta e valor total arrecadado.
/// </summary>
[ApiController]
[Route("api/publico/campanhas")]
[AllowAnonymous]
public class PublicoController(ICampanhaService campanhas) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CampanhaPublicaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CampanhaPublicaResponse>>> Painel(CancellationToken ct)
        => Ok(await campanhas.ListarPainelPublicoAsync(ct));

    /// <summary>Status de uma campanha — consumido pelo microsserviço de Doações.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CampanhaStatusResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampanhaStatusResponse>> ObterStatus(Guid id, CancellationToken ct)
        => Ok(await campanhas.ObterStatusPublicoAsync(id, ct));
}
