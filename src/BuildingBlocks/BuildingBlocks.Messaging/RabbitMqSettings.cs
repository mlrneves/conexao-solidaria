namespace BuildingBlocks.Messaging;

/// <summary>Configuração da mensageria (seção "RabbitMq").</summary>
public class RabbitMqSettings
{
    public const string Secao = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "conexao";
    public string Pass { get; set; } = "conexao-dev";

    public string Exchange { get; set; } = "conexao.eventos";
    public string RoutingKeyDoacaoRecebida { get; set; } = "doacao.recebida";
    public string QueueDoacoes { get; set; } = "doacoes.processamento";

    // Dead-letter: mensagens que esgotam as tentativas de processamento vão
    // para a DLQ, preservadas para inspeção na Management UI.
    public string DlxExchange { get; set; } = "conexao.eventos.dlx";
    public string DlqQueue { get; set; } = "doacoes.processamento.dlq";
}
