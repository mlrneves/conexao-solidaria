using BuildingBlocks.Contracts.Events;

namespace Doacoes.Worker.Persistence;

public enum ResultadoIncremento
{
    /// <summary>Total da campanha incrementado com sucesso.</summary>
    Incrementada,

    /// <summary>Campanha não está mais Ativa — doação deve ser rejeitada.</summary>
    CampanhaNaoAtiva,

    /// <summary>Evento redelivered: doação já registrada antes (idempotência).</summary>
    JaProcessada
}

// SOLID (DIP): o processador orquestra sobre esta abstração; o acesso ao
// Postgres fica isolado na implementação (testável com mocks).
public interface IProcessamentoRepository
{
    Task GarantirTabelaControleAsync(CancellationToken ct = default);

    /// <summary>
    /// Operação ATÔMICA (uma transação): registra o DoacaoId na tabela de
    /// controle (dedup) e incrementa o total da campanha se ela estiver Ativa.
    /// </summary>
    Task<ResultadoIncremento> RegistrarEIncrementarAsync(
        DoacaoRecebidaEvent evento, CancellationToken ct = default);
}
