using BuildingBlocks.Contracts.Events;
using Doacoes.Worker.Persistence;
using Doacoes.Worker.Processing;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Doacoes.Tests;

public class ProcessadorDeDoacaoTests
{
    private readonly IProcessamentoRepository _repositorio = Substitute.For<IProcessamentoRepository>();
    private readonly IStatusDoacaoAtualizador _status = Substitute.For<IStatusDoacaoAtualizador>();
    private readonly ProcessadorDeDoacao _processador;

    private static readonly DoacaoRecebidaEvent Evento = new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 75m, DateTime.UtcNow);

    public ProcessadorDeDoacaoTests()
    {
        _processador = new ProcessadorDeDoacao(
            _repositorio, _status, Substitute.For<ILogger<ProcessadorDeDoacao>>());
    }

    [Fact]
    public async Task Processar_ComIncrementoRealizado_MarcaProcessadaNoMongo()
    {
        _repositorio.RegistrarEIncrementarAsync(Evento, Arg.Any<CancellationToken>())
            .Returns(ResultadoIncremento.Incrementada);

        var resultado = await _processador.ProcessarAsync(Evento);

        Assert.Equal(ResultadoProcessamento.Processada, resultado);
        await _status.Received(1).MarcarProcessadaAsync(
            Evento.DoacaoId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Processar_ComCampanhaNaoAtiva_MarcaRejeitadaComMotivo()
    {
        _repositorio.RegistrarEIncrementarAsync(Evento, Arg.Any<CancellationToken>())
            .Returns(ResultadoIncremento.CampanhaNaoAtiva);

        var resultado = await _processador.ProcessarAsync(Evento);

        Assert.Equal(ResultadoProcessamento.Rejeitada, resultado);
        await _status.Received(1).MarcarRejeitadaAsync(
            Evento.DoacaoId, Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _status.DidNotReceive().MarcarProcessadaAsync(
            Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Processar_EventoDuplicado_NaoTocaNoMongo()
    {
        // Idempotência: redelivery do RabbitMQ não pode somar duas vezes
        // nem sobrescrever o status já gravado.
        _repositorio.RegistrarEIncrementarAsync(Evento, Arg.Any<CancellationToken>())
            .Returns(ResultadoIncremento.JaProcessada);

        var resultado = await _processador.ProcessarAsync(Evento);

        Assert.Equal(ResultadoProcessamento.Duplicada, resultado);
        await _status.DidNotReceive().MarcarProcessadaAsync(
            Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
        await _status.DidNotReceive().MarcarRejeitadaAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Processar_QuandoRepositorioFalha_PropagaExcecao()
    {
        // O consumer usa a exceção para acionar retry e, no limite, a DLQ.
        _repositorio.RegistrarEIncrementarAsync(Evento, Arg.Any<CancellationToken>())
            .Returns<ResultadoIncremento>(_ => throw new InvalidOperationException("banco fora"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _processador.ProcessarAsync(Evento));
        await _status.DidNotReceive().MarcarProcessadaAsync(
            Arg.Any<Guid>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }
}
