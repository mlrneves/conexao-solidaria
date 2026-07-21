using System.Security.Claims;
using Doacoes.Api.Dtos;
using Doacoes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doacoes.Api.Controllers;

/// <summary>
/// Processo de doação — somente Doador logado (requisito 5 do edital).
/// </summary>
[ApiController]
[Route("api/doacoes")]
[Authorize(Roles = "Doador")]
public class DoacoesController(IDoacaoService doacoes) : ControllerBase
{
    /// <summary>
    /// Envia a intenção de doação. Retorna 202 Accepted: o processamento é
    /// assíncrono (RabbitMQ → Worker atualiza o total da campanha).
    /// </summary>
    [HttpPost]
    [ProducesResponseType<DoacaoAceitaResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<DoacaoAceitaResponse>> Doar(DoarRequest request, CancellationToken ct)
    {
        var doadorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resposta = await doacoes.DoarAsync(request, doadorId, ct);
        return Accepted(resposta);
    }

    /// <summary>Minhas doações — mostra o status Pendente → Processada (assincronismo visível).</summary>
    [HttpGet("minhas")]
    [ProducesResponseType<IReadOnlyList<MinhaDoacaoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MinhaDoacaoResponse>>> Minhas(CancellationToken ct)
    {
        var doadorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await doacoes.ListarMinhasAsync(doadorId, ct));
    }
}
