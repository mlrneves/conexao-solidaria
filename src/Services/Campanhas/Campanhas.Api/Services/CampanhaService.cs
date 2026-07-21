using Campanhas.Api.Dtos;
using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.Repositories;

namespace Campanhas.Api.Services;

public class CampanhaService(ICampanhaRepository campanhas) : ICampanhaService
{
    public async Task<CampanhaResponse> CriarAsync(
        CriarCampanhaRequest request, Guid gestorId, CancellationToken ct)
    {
        var campanha = Campanha.Criar(
            request.Titulo, request.Descricao,
            ParaUtc(request.DataInicio), ParaUtc(request.DataFim),
            request.MetaFinanceira, gestorId, DateTime.UtcNow);

        await campanhas.AdicionarAsync(campanha, ct);
        await campanhas.SalvarAlteracoesAsync(ct);
        return Mapear(campanha);
    }

    public async Task<CampanhaResponse> AtualizarAsync(
        Guid id, AtualizarCampanhaRequest request, CancellationToken ct)
    {
        var campanha = await campanhas.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Campanha não encontrada.");

        campanha.Atualizar(
            request.Titulo, request.Descricao,
            ParaUtc(request.DataInicio), ParaUtc(request.DataFim),
            request.MetaFinanceira, request.Status);

        await campanhas.SalvarAlteracoesAsync(ct);
        return Mapear(campanha);
    }

    public async Task<IReadOnlyList<CampanhaResponse>> ListarTodasAsync(CancellationToken ct)
        => [.. (await campanhas.ListarTodasAsync(ct)).Select(Mapear)];

    public async Task<CampanhaResponse> ObterPorIdAsync(Guid id, CancellationToken ct)
        => Mapear(await campanhas.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Campanha não encontrada."));

    public async Task<IReadOnlyList<CampanhaPublicaResponse>> ListarPainelPublicoAsync(CancellationToken ct)
        => [.. (await campanhas.ListarPorStatusAsync(StatusCampanha.Ativa, ct))
            .Select(c => new CampanhaPublicaResponse(
                c.Id, c.Titulo, c.Descricao, c.MetaFinanceira, c.ValorTotalArrecadado, c.DataFim))];

    public async Task<CampanhaStatusResponse> ObterStatusPublicoAsync(Guid id, CancellationToken ct)
    {
        var campanha = await campanhas.ObterPorIdAsync(id, ct)
            ?? throw new RecursoNaoEncontradoException("Campanha não encontrada.");
        return new CampanhaStatusResponse(campanha.Id, campanha.Titulo, campanha.Status);
    }

    /// <summary>Npgsql exige DateTime UTC em colunas timestamptz — normaliza o Kind vindo do JSON.</summary>
    private static DateTime ParaUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
    };

    private static CampanhaResponse Mapear(Campanha c) => new(
        c.Id, c.Titulo, c.Descricao, c.DataInicio, c.DataFim,
        c.MetaFinanceira, c.Status, c.ValorTotalArrecadado, c.CriadoEmUtc);
}
