using BuildingBlocks.Contracts.Events;
using Microsoft.EntityFrameworkCore;

namespace Doacoes.Worker.Persistence;

public class ProcessamentoRepository(WorkerDbContext db) : IProcessamentoRepository
{
    public Task GarantirTabelaControleAsync(CancellationToken ct = default)
        => db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS doacoes_processadas (
                "DoacaoId" uuid PRIMARY KEY,
                "ProcessadaEmUtc" timestamptz NOT NULL
            )
            """, ct);

    public async Task<ResultadoIncremento> RegistrarEIncrementarAsync(
        DoacaoRecebidaEvent evento, CancellationToken ct = default)
    {
        // Idempotência + incremento na MESMA transação: um redelivery do
        // RabbitMQ jamais soma duas vezes.
        await using var transacao = await db.Database.BeginTransactionAsync(ct);

        var inseridas = await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO doacoes_processadas ("DoacaoId", "ProcessadaEmUtc")
             VALUES ({evento.DoacaoId}, {DateTime.UtcNow})
             ON CONFLICT DO NOTHING
             """, ct);

        if (inseridas == 0)
        {
            await transacao.RollbackAsync(ct);
            return ResultadoIncremento.JaProcessada;
        }

        // UPDATE atômico e condicional: sem read-modify-write (não há corrida
        // entre réplicas do worker) e revalida a regra de negócio — a campanha
        // pode ter sido cancelada entre o aceite da doação e o processamento.
        var linhas = await db.Campanhas
            .Where(c => c.Id == evento.CampanhaId && c.Status == "Ativa")
            .ExecuteUpdateAsync(s => s.SetProperty(
                c => c.ValorTotalArrecadado,
                c => c.ValorTotalArrecadado + evento.Valor), ct);

        await transacao.CommitAsync(ct);

        return linhas == 1
            ? ResultadoIncremento.Incrementada
            : ResultadoIncremento.CampanhaNaoAtiva;
    }
}
