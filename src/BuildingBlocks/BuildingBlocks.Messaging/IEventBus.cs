namespace BuildingBlocks.Messaging;

// SOLID (DIP/OCP): os serviços publicam eventos por esta abstração; trocar
// RabbitMQ por Kafka significa apenas escrever outra implementação — nenhum
// consumidor de IEventBus muda.
public interface IEventBus
{
    Task PublicarAsync<TEvento>(TEvento evento, string routingKey, CancellationToken ct = default)
        where TEvento : class;
}
