using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging;

// SOLID (SRP): esta classe só publica eventos — serialização JSON, mensagem
// persistente e publisher confirms. Consumo fica no Worker.
public class RabbitMqEventBus(IConnection conexao, RabbitMqSettings settings, ILogger<RabbitMqEventBus> logger)
    : IEventBus
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task PublicarAsync<TEvento>(
        TEvento evento, string routingKey, CancellationToken ct = default)
        where TEvento : class
    {
        // Canal por publicação: canais não são thread-safe; o custo é baixo
        // para o volume do MVP e evita sincronização manual.
        await using var canal = await conexao.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            ct);

        await RabbitMqTopology.DeclararAsync(canal, settings, ct);

        var corpo = JsonSerializer.SerializeToUtf8Bytes(evento, JsonOptions);
        var props = new BasicProperties
        {
            Persistent = true, // sobrevive a restart do broker (fila durável)
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Type = typeof(TEvento).Name
        };

        await canal.BasicPublishAsync(
            settings.Exchange, routingKey, mandatory: true,
            basicProperties: props, body: corpo, cancellationToken: ct);

        logger.LogInformation(
            "Evento {Evento} publicado em {Exchange}/{RoutingKey}",
            typeof(TEvento).Name, settings.Exchange, routingKey);
    }
}
