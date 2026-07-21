using Campanhas.Domain.Exceptions;
using Campanhas.Domain.ValueObjects;

namespace Campanhas.Domain.Tests;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData(" 529.982.247-25 ")]
    public void Criar_ComCpfValido_NormalizaParaSomenteDigitos(string entrada)
    {
        var cpf = Cpf.Criar(entrada);
        Assert.Equal("52998224725", cpf.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456789012")]
    public void Criar_ComTamanhoInvalido_LancaRegraDeNegocio(string entrada)
    {
        Assert.Throws<RegraDeNegocioException>(() => Cpf.Criar(entrada));
    }

    [Theory]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    public void Criar_ComDigitosRepetidos_LancaRegraDeNegocio(string entrada)
    {
        Assert.Throws<RegraDeNegocioException>(() => Cpf.Criar(entrada));
    }

    [Theory]
    [InlineData("52998224724")] // segundo dígito verificador errado
    [InlineData("52998224735")] // primeiro dígito verificador errado
    public void Criar_ComDigitoVerificadorInvalido_LancaRegraDeNegocio(string entrada)
    {
        Assert.Throws<RegraDeNegocioException>(() => Cpf.Criar(entrada));
    }

    [Fact]
    public void Mascarado_ExibeSomentePrimeirosTresEUltimosDoisDigitos()
    {
        var cpf = Cpf.Criar("529.982.247-25");
        Assert.Equal("529.***.***-25", cpf.Mascarado());
    }
}
