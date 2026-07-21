using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Messaging;
using Doacoes.Api.Domain;
using Doacoes.Api.Domain.Exceptions;
using Doacoes.Api.Dtos;
using Doacoes.Api.Infrastructure;
using Doacoes.Api.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Doacoes.Tests;

public class DoacaoServiceTests
{
    private readonly IDoacaoRepository _repositorio = Substitute.For<IDoacaoRepository>();
    private readonly ICampanhasClient _campanhas = Substitute.For<ICampanhasClient>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly DoacaoService _service;

    private static readonly Guid CampanhaId = Guid.NewGuid();
    private static readonly Guid DoadorId = Guid.NewGuid();

    public DoacaoServiceTests()
    {
        _service = new DoacaoService(
            _repositorio, _campanhas, _eventBus, new RabbitMqSettings(),
            Substitute.For<ILogger<DoacaoService>>());
    }

    [Fact]
    public async Task Doar_EmCampanhaAtiva_PersisteEPublicaEventoERetorna202Pendente()
    {
        _campanhas.ObterCampanhaAsync(CampanhaId, Arg.Any<CancellationToken>())
            .Returns(new CampanhaStatusDto(CampanhaId, "Natal Solidário", "Ativa"));

        var resposta = await _service.DoarAsync(
            new DoarRequest { CampanhaId = CampanhaId, Valor = 50m }, DoadorId, CancellationToken.None);

        Assert.Equal(StatusDoacao.Pendente, resposta.Status);
        await _repositorio.Received(1).AdicionarAsync(
            Arg.Is<Doacao>(d => d.CampanhaId == CampanhaId && d.Valor == 50m),
            Arg.Any<CancellationToken>());
        // Requisito do edital: a API publica o evento — NÃO atualiza o total.
        await _eventBus.Received(1).PublicarAsync(
            Arg.Is<DoacaoRecebidaEvent>(e => e.CampanhaId == CampanhaId && e.Valor == 50m),
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Concluida")]
    [InlineData("Cancelada")]
    public async Task Doar_EmCampanhaEncerradaOuCancelada_Lanca422ENaoPublica(string status)
    {
        _campanhas.ObterCampanhaAsync(CampanhaId, Arg.Any<CancellationToken>())
            .Returns(new CampanhaStatusDto(CampanhaId, "Campanha", status));

        await Assert.ThrowsAsync<CampanhaNaoDoavelException>(() => _service.DoarAsync(
            new DoarRequest { CampanhaId = CampanhaId, Valor = 50m }, DoadorId, CancellationToken.None));

        await _repositorio.DidNotReceive().AdicionarAsync(
            Arg.Any<Doacao>(), Arg.Any<CancellationToken>());
        await _eventBus.DidNotReceive().PublicarAsync(
            Arg.Any<DoacaoRecebidaEvent>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Doar_EmCampanhaInexistente_Lanca404()
    {
        _campanhas.ObterCampanhaAsync(CampanhaId, Arg.Any<CancellationToken>())
            .Returns((CampanhaStatusDto?)null);

        await Assert.ThrowsAsync<CampanhaNaoEncontradaException>(() => _service.DoarAsync(
            new DoarRequest { CampanhaId = CampanhaId, Valor = 50m }, DoadorId, CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Doar_ComValorInvalido_Lanca400SemConsultarCampanhas(decimal valor)
    {
        await Assert.ThrowsAsync<DoacaoInvalidaException>(() => _service.DoarAsync(
            new DoarRequest { CampanhaId = CampanhaId, Valor = valor }, DoadorId, CancellationToken.None));

        await _campanhas.DidNotReceive().ObterCampanhaAsync(
            Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
