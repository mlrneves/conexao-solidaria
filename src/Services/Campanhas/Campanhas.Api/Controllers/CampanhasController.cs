using System.Security.Claims;
using Campanhas.Api.Dtos;
using Campanhas.Api.Services;
using Campanhas.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Controllers;

/// <summary>
/// Gestão de campanhas — acesso exclusivo da role GestorONG (RBAC, requisito 2).
/// Um Doador autenticado recebe 403 Forbidden aqui.
/// </summary>
[ApiController]
[Route("api/campanhas")]
[Authorize(Roles = nameof(PerfilUsuario.GestorONG))]
public class CampanhasController(ICampanhaService campanhas) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CampanhaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CampanhaResponse>>> Listar(CancellationToken ct)
        => Ok(await campanhas.ListarTodasAsync(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CampanhaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampanhaResponse>> ObterPorId(Guid id, CancellationToken ct)
        => Ok(await campanhas.ObterPorIdAsync(id, ct));

    /// <summary>
    /// Cria campanha. Regras: data de término não pode estar no passado e a
    /// meta financeira deve ser maior que zero (400 em caso de violação).
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CampanhaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CampanhaResponse>> Criar(
        CriarCampanhaRequest request, CancellationToken ct)
    {
        var gestorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var campanha = await campanhas.CriarAsync(request, gestorId, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = campanha.Id }, campanha);
    }

    /// <summary>Edita a campanha, incluindo mudança de status (Concluida/Cancelada).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<CampanhaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampanhaResponse>> Atualizar(
        Guid id, AtualizarCampanhaRequest request, CancellationToken ct)
        => Ok(await campanhas.AtualizarAsync(id, request, ct));
}
