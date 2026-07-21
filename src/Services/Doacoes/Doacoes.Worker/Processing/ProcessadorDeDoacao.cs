using BuildingBlocks.Contracts.Events;
using Doacoes.Worker.Persistence;
using Prometheus;

namespace Doacoes.Worker.Processing;

public enum ResultadoProcessamento
{
    Processada,
    Rejeitada,
    Duplicada
}

public interface IProcessadorDeDoacao
{
    Task<ResultadoProcessamento> ProcessarAsync(DoacaoRecebidaEvent evento, CancellationToken ct = default);
}

// SOLID (SRP): orquestra o processamento de UM evento — incremento no
// Postgres, status no Mongo e métrica. Não conhece RabbitMQ (isso é do
// consumer) nem SQL (isso é do repositório) — logo, é testável com mocks.
public class ProcessadorDeDoacao(
    IProcessamentoRepository repositorio,
    IStatusDoacaoAtualizador statusDoacao,
    ILogger<ProcessadorDeDoacao> logger) : IProcessadorDeDoacao
{
    /// <summary>Métrica de negócio exibida no dashboard do Grafana.</summary>
    private static readonly Counter DoacoesProcessadas = Metrics.CreateCounter(
        "conexao_doacoes_processadas_total",
        "Total de doações processadas pelo Worker, por resultado.",
        "resultado");

    public async Task<ResultadoProcessamento> ProcessarAsync(
        DoacaoRecebidaEvent evento, CancellationToken ct = default)
    {
        var resultado = await repositorio.RegistrarEIncrementarAsync(evento, ct);

        switch (resultado)
        {
            case ResultadoIncremento.Incrementada:
                await statusDoacao.MarcarProcessadaAsync(evento.DoacaoId, DateTime.UtcNow, ct);
                DoacoesProcessadas.WithLabels("sucesso").Inc();
                logger.LogInformation(
                    "Doação {DoacaoId} processada: campanha {CampanhaId} += {Valor}",
                    evento.DoacaoId, evento.CampanhaId, evento.Valor);
                return ResultadoProcessamento.Processada;

            case ResultadoIncremento.CampanhaNaoAtiva:
                await statusDoacao.MarcarRejeitadaAsync(
                    evento.DoacaoId, "Campanha não está mais ativa.", DateTime.UtcNow, ct);
                DoacoesProcessadas.WithLabels("rejeitada").Inc();
                logger.LogWarning(
                    "Doação {DoacaoId} rejeitada: campanha {CampanhaId} não está ativa",
                    evento.DoacaoId, evento.CampanhaId);
                return ResultadoProcessamento.Rejeitada;

            default:
                // Redelivery: nada a fazer — o total já foi somado antes.
                DoacoesProcessadas.WithLabels("duplicada").Inc();
                logger.LogInformation(
                    "Doação {DoacaoId} já havia sido processada (idempotência)", evento.DoacaoId);
                return ResultadoProcessamento.Duplicada;
        }
    }
}
