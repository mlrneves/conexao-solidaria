namespace BuildingBlocks.Contracts.Events;

/// <summary>
/// Evento de integração publicado pelo Doacoes.Api quando uma intenção de
/// doação é aceita, e consumido pelo Doacoes.Worker para atualizar o valor
/// total arrecadado da campanha (requisito de mensageria do edital).
/// </summary>
public record DoacaoRecebidaEvent(
    Guid DoacaoId,
    Guid CampanhaId,
    Guid DoadorId,
    decimal Valor,
    DateTime OcorridoEmUtc);
