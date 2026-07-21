using System.Text.Json;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging;
using Doacoes.Worker.Processing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Doacoes.Worker.Consumers;

/// <summary>
/// Consumer da fila doacoes.processamento (ack manual, prefetch 5).
/// Retry in-process 3x com backoff (1s/3s/9s); esgotado → BasicNack sem
/// requeue → a mensagem cai na DLQ via dead-letter-exchange.
/// </summary>
public class DoacaoRecebidaConsumer(
    IConnection conexao,
    RabbitMqSettings settings,
    IServiceScopeFactory scopeFactory,
    ILogger<DoacaoRecebidaConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private static readonly TimeSpan[] Backoff =
        [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(9)];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var canal = await conexao.CreateChannelAsync(cancellationToken: stoppingToken);
        await RabbitMqTopology.DeclararAsync(canal, settings, stoppingToken);
        await canal.BasicQosAsync(prefetchSize: 0, prefetchCount: 5, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(canal);
        consumer.ReceivedAsync += async (_, entrega) =>
        {
            DoacaoRecebidaEvent? evento;
            try
            {
                evento = JsonSerializer.Deserialize<DoacaoRecebidaEvent>(entrega.Body.Span, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Mensagem com JSON inválido — enviando direto para a DLQ");
                await canal.BasicNackAsync(entrega.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (evento is null)
            {
                await canal.BasicNackAsync(entrega.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            for (var tentativa = 1; tentativa <= Backoff.Length; tentativa++)
            {
                try
                {
                    // Escopo por mensagem: DbContext e dependências scoped novos.
                    using var scope = scopeFactory.CreateScope();
                    var processador = scope.ServiceProvider.GetRequiredService<IProcessadorDeDoacao>();
                    await processador.ProcessarAsync(evento, stoppingToken);
                    await canal.BasicAckAsync(entrega.DeliveryTag, multiple: false);
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Falha ao processar doação {DoacaoId} (tentativa {Tentativa}/{Total})",
                        evento.DoacaoId, tentativa, Backoff.Length);

                    if (tentativa < Backoff.Length)
                        await Task.Delay(Backoff[tentativa - 1], stoppingToken);
                }
            }

            logger.LogError(
                "Doação {DoacaoId}: tentativas esgotadas — mensagem enviada para a DLQ {Dlq}",
                evento.DoacaoId, settings.DlqQueue);
            await canal.BasicNackAsync(entrega.DeliveryTag, multiple: false, requeue: false);
        };

        await canal.BasicConsumeAsync(settings.QueueDoacoes, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Worker consumindo a fila {Fila}", settings.QueueDoacoes);

        // Mantém o BackgroundService vivo até o shutdown do host.
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown normal
        }
    }
}
