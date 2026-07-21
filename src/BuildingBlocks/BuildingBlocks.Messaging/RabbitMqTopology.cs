using RabbitMQ.Client;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Declaração idempotente da topologia: exchange principal (direct) → fila de
/// processamento com dead-letter-exchange → DLQ. Publisher e consumer chamam
/// no startup — quem subir primeiro cria, os demais apenas confirmam.
/// </summary>
public static class RabbitMqTopology
{
    public static async Task DeclararAsync(IChannel canal, RabbitMqSettings s, CancellationToken ct = default)
    {
        await canal.ExchangeDeclareAsync(s.Exchange, ExchangeType.Direct,
            durable: true, autoDelete: false, arguments: null, cancellationToken: ct);

        await canal.ExchangeDeclareAsync(s.DlxExchange, ExchangeType.Fanout,
            durable: true, autoDelete: false, arguments: null, cancellationToken: ct);

        await canal.QueueDeclareAsync(s.DlqQueue, durable: true, exclusive: false,
            autoDelete: false, arguments: null, cancellationToken: ct);
        await canal.QueueBindAsync(s.DlqQueue, s.DlxExchange, routingKey: string.Empty,
            arguments: null, cancellationToken: ct);

        var argumentosFila = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = s.DlxExchange
        };
        await canal.QueueDeclareAsync(s.QueueDoacoes, durable: true, exclusive: false,
            autoDelete: false, arguments: argumentosFila, cancellationToken: ct);
        await canal.QueueBindAsync(s.QueueDoacoes, s.Exchange, s.RoutingKeyDoacaoRecebida,
            arguments: null, cancellationToken: ct);
    }
}
