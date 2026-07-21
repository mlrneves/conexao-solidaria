using RabbitMQ.Client;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Cria a conexão AMQP com retry — no Kubernetes/Compose o broker pode subir
/// depois dos serviços. A conexão é única por processo (singleton no DI) e o
/// client faz recovery automático se cair depois.
/// </summary>
public static class RabbitMqConnector
{
    public static async Task<IConnection> ConectarComRetryAsync(
        RabbitMqSettings s, Action<string> log, int tentativas = 12)
    {
        var factory = new ConnectionFactory
        {
            HostName = s.Host,
            Port = s.Port,
            UserName = s.User,
            Password = s.Pass,
            AutomaticRecoveryEnabled = true
        };

        for (var i = 1; ; i++)
        {
            try
            {
                return await factory.CreateConnectionAsync();
            }
            catch (Exception ex) when (i < tentativas)
            {
                log($"RabbitMQ indisponível em {s.Host}:{s.Port} " +
                    $"(tentativa {i}/{tentativas}): {ex.Message}. Aguardando 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}
