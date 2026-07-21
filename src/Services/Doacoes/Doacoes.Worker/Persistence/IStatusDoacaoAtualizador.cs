namespace Doacoes.Worker.Persistence;

/// <summary>Atualiza o status da doação no banco do serviço de Doações (MongoDB).</summary>
public interface IStatusDoacaoAtualizador
{
    Task MarcarProcessadaAsync(Guid doacaoId, DateTime quandoUtc, CancellationToken ct = default);
    Task MarcarRejeitadaAsync(Guid doacaoId, string motivo, DateTime quandoUtc, CancellationToken ct = default);
}
