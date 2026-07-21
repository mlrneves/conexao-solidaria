using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.ValueObjects;

namespace Campanhas.Domain.Tests;

public class UsuarioTests
{
    private static readonly DateTime Agora = new(2026, 07, 21, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CriarDoador_SemConsentimentoLgpd_LancaRegraDeNegocio()
    {
        // LGPD: sem consentimento explícito não há cadastro.
        var ex = Assert.Throws<RegraDeNegocioException>(() => Usuario.CriarDoador(
            "Maria da Silva", "maria@email.com", Cpf.Criar("52998224725"),
            "hash", consentimentoLgpd: false, Agora));

        Assert.Contains("LGPD", ex.Message);
    }

    [Fact]
    public void CriarDoador_ComConsentimento_RegistraMomentoDoConsentimento()
    {
        var doador = Usuario.CriarDoador(
            "Maria da Silva", "  MARIA@Email.com ", Cpf.Criar("52998224725"),
            "hash", consentimentoLgpd: true, Agora);

        Assert.Equal(PerfilUsuario.Doador, doador.Perfil);
        Assert.True(doador.ConsentimentoLgpd);
        Assert.Equal(Agora, doador.DataConsentimentoUtc);
        Assert.Equal("maria@email.com", doador.Email); // e-mail normalizado
    }

    [Fact]
    public void CriarGestor_TemPerfilGestorONG()
    {
        var gestor = Usuario.CriarGestor("Gestor", "gestor@ong.org", "hash", Agora);
        Assert.Equal(PerfilUsuario.GestorONG, gestor.Perfil);
    }
}
