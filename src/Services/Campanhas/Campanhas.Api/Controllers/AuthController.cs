using System.Security.Claims;
using Campanhas.Api.Dtos;
using Campanhas.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Cadastro público de doador (requisito 3 do edital).</summary>
    [HttpPost("registrar")]
    [AllowAnonymous]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Registrar(
        RegistrarDoadorRequest request, CancellationToken ct)
    {
        var usuario = await auth.RegistrarDoadorAsync(request, ct);
        return CreatedAtAction(nameof(Me), null, usuario);
    }

    /// <summary>Autenticação: retorna o token JWT com a role do usuário (requisito 1).</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var resposta = await auth.LoginAsync(request, ct);
        if (resposta is null)
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciais inválidas.",
                Status = StatusCodes.Status401Unauthorized
            });
        return Ok(resposta);
    }

    /// <summary>Dados do próprio titular, com CPF mascarado (LGPD).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await auth.ObterMeAsync(id, ct));
    }
}
