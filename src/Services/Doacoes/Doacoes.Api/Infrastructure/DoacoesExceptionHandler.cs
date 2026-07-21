using Doacoes.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Doacoes.Api.Infrastructure;

/// <summary>
/// DoacaoInvalida → 400 | CampanhaNaoEncontrada → 404 | CampanhaNaoDoavel → 422.
/// </summary>
public class DoacoesExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, titulo) = exception switch
        {
            DoacaoInvalidaException => (StatusCodes.Status400BadRequest, "Doação inválida"),
            CampanhaNaoEncontradaException => (StatusCodes.Status404NotFound, "Campanha não encontrada"),
            CampanhaNaoDoavelException => (StatusCodes.Status422UnprocessableEntity, "Doação não permitida"),
            _ => (0, string.Empty)
        };

        if (status == 0) return false;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = exception.Message
        }, ct);
        return true;
    }
}
