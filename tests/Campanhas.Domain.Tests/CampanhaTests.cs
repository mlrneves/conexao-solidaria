using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Exceptions;

namespace Campanhas.Domain.Tests;

public class CampanhaTests
{
    private static readonly DateTime Agora = new(2026, 07, 21, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid Gestor = Guid.NewGuid();

    private static Campanha CriarValida() => Campanha.Criar(
        "Natal Solidário", "Arrecadação para as crianças acolhidas.",
        Agora, Agora.AddDays(30), 10_000m, Gestor, Agora);

    [Fact]
    public void Criar_ComDadosValidos_ComecaAtivaESemArrecadacao()
    {
        var campanha = CriarValida();

        Assert.Equal(StatusCampanha.Ativa, campanha.Status);
        Assert.Equal(0m, campanha.ValorTotalArrecadado);
        Assert.Equal("Natal Solidário", campanha.Titulo);
        Assert.NotEqual(Guid.Empty, campanha.Id);
    }

    [Fact]
    public void Criar_ComDataFimNoPassado_LancaRegraDeNegocio()
    {
        // Regra do edital: campanha não pode ser criada com data de término no passado.
        var ex = Assert.Throws<RegraDeNegocioException>(() => Campanha.Criar(
            "Título", "Descrição", Agora.AddDays(-10), Agora.AddDays(-1),
            1000m, Gestor, Agora));

        Assert.Contains("passado", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-500.50)]
    public void Criar_ComMetaMenorOuIgualAZero_LancaRegraDeNegocio(decimal meta)
    {
        // Regra do edital: a meta financeira deve ser maior que zero.
        Assert.Throws<RegraDeNegocioException>(() => Campanha.Criar(
            "Título", "Descrição", Agora, Agora.AddDays(30), meta, Gestor, Agora));
    }

    [Fact]
    public void Criar_ComDataFimAnteriorAoInicio_LancaRegraDeNegocio()
    {
        Assert.Throws<RegraDeNegocioException>(() => Campanha.Criar(
            "Título", "Descrição", Agora.AddDays(10), Agora.AddDays(5),
            1000m, Gestor, Agora));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemTitulo_LancaRegraDeNegocio(string titulo)
    {
        Assert.Throws<RegraDeNegocioException>(() => Campanha.Criar(
            titulo, "Descrição", Agora, Agora.AddDays(30), 1000m, Gestor, Agora));
    }

    [Fact]
    public void Criar_ComTituloAcimaDe150Caracteres_LancaRegraDeNegocio()
    {
        Assert.Throws<RegraDeNegocioException>(() => Campanha.Criar(
            new string('a', 151), "Descrição", Agora, Agora.AddDays(30), 1000m, Gestor, Agora));
    }

    [Fact]
    public void Atualizar_PermiteConcluirCampanha()
    {
        var campanha = CriarValida();

        campanha.Atualizar(campanha.Titulo, campanha.Descricao,
            campanha.DataInicio, campanha.DataFim, campanha.MetaFinanceira,
            StatusCampanha.Concluida);

        Assert.Equal(StatusCampanha.Concluida, campanha.Status);
    }

    [Fact]
    public void Atualizar_ComMetaInvalida_LancaRegraDeNegocio()
    {
        var campanha = CriarValida();

        Assert.Throws<RegraDeNegocioException>(() => campanha.Atualizar(
            campanha.Titulo, campanha.Descricao, campanha.DataInicio,
            campanha.DataFim, 0m, StatusCampanha.Ativa));
    }
}
