using Campanhas.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Campanhas.Api.Infrastructure;

/// <summary>
/// Converte exceções de domínio em ProblemDetails com o status HTTP adequado:
/// RegraDeNegocio → 400, Conflito → 409, RecursoNaoEncontrado → 404.
/// </summary>
public class DominioExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, titulo) = exception switch
        {
            RegraDeNegocioException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            ConflitoException => (StatusCodes.Status409Conflict, "Conflito"),
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            _ => (0, string.Empty)
        };

        if (status == 0) return false; // deixa o handler padrão tratar (500)

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
