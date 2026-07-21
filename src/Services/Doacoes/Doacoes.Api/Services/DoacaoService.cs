using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging;
using Doacoes.Api.Domain;
using Doacoes.Api.Domain.Exceptions;
using Doacoes.Api.Dtos;
using Doacoes.Api.Infrastructure;

namespace Doacoes.Api.Services;

// SOLID (DIP): depende só de abstrações (repositório, client HTTP, event bus).
// O fluxo materializa o requisito central do edital: a API NÃO atualiza o
// total arrecadado — publica DoacaoRecebidaEvent e responde 202.
public class DoacaoService(
    IDoacaoRepository doacoes,
    ICampanhasClient campanhas,
    IEventBus eventBus,
    RabbitMqSettings rabbitSettings,
    ILogger<DoacaoService> logger) : IDoacaoService
{
    public async Task<DoacaoAceitaResponse> DoarAsync(
        DoarRequest request, Guid doadorId, CancellationToken ct)
    {
        // 1. Validação local (valor > 0 etc.) na fábrica da entidade.
        var doacao = Doacao.Criar(request.CampanhaId, doadorId, request.Valor, DateTime.UtcNow);

        // 2. Validação cross-service: a campanha existe e está ativa?
        var campanha = await campanhas.ObterCampanhaAsync(request.CampanhaId, ct)
            ?? throw new CampanhaNaoEncontradaException("Campanha não encontrada.");

        if (!campanha.EstaAtiva)
            throw new CampanhaNaoDoavelException(
                "Não é possível doar para campanhas encerradas ou canceladas.");

        // 3. Persiste a intenção (Pendente) no MongoDB.
        await doacoes.AdicionarAsync(doacao, ct);

        // 4. Publica o evento — o Worker fará a soma no banco de Campanhas.
        //    (Trade-off outbox: gravou-mas-não-publicou é possível; documentado
        //    em docs/ARQUITETURA.md como evolução.)
        var evento = new DoacaoRecebidaEvent(
            doacao.Id, doacao.CampanhaId, doacao.DoadorId, doacao.Valor, doacao.CriadaEmUtc);
        await eventBus.PublicarAsync(evento, rabbitSettings.RoutingKeyDoacaoRecebida, ct);

        logger.LogInformation(
            "Doação {DoacaoId} de {Valor:C} aceita para campanha {CampanhaId} — evento publicado",
            doacao.Id, doacao.Valor, doacao.CampanhaId);

        return new DoacaoAceitaResponse(
            doacao.Id, doacao.Status,
            "Doação recebida e em processamento. Acompanhe em 'minhas doações'.");
    }

    public async Task<IReadOnlyList<MinhaDoacaoResponse>> ListarMinhasAsync(
        Guid doadorId, CancellationToken ct)
        => [.. (await doacoes.ListarPorDoadorAsync(doadorId, ct))
            .Select(d => new MinhaDoacaoResponse(
                d.Id, d.CampanhaId, d.Valor, d.Status,
                d.CriadaEmUtc, d.ProcessadaEmUtc, d.MotivoRejeicao))];
}
