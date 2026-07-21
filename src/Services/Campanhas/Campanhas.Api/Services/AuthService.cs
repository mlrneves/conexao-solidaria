using Campanhas.Api.Dtos;
using Campanhas.Domain.Entities;
using Campanhas.Domain.Enums;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.Repositories;
using Campanhas.Domain.ValueObjects;

namespace Campanhas.Api.Services;

// SOLID (DIP): depende apenas das abstrações IUsuarioRepository/ITokenService;
// os concretos são amarrados na raiz de composição (Program.cs).
public class AuthService(IUsuarioRepository usuarios, ITokenService tokens) : IAuthService
{
    public async Task<UsuarioResponse> RegistrarDoadorAsync(
        RegistrarDoadorRequest request, CancellationToken ct)
    {
        var cpf = Cpf.Criar(request.Cpf);
        var email = Usuario.NormalizarEmail(request.Email);

        if (await usuarios.EmailExisteAsync(email, ct))
            throw new ConflitoException("Já existe um cadastro com este e-mail.");

        // Senha nunca é armazenada em claro — hash BCrypt (requisito do edital + LGPD).
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

        var doador = Usuario.CriarDoador(
            request.NomeCompleto, email, cpf, senhaHash,
            request.ConsentimentoLgpd, DateTime.UtcNow);

        await usuarios.AdicionarAsync(doador, ct);
        await usuarios.SalvarAlteracoesAsync(ct);

        return new UsuarioResponse(doador.Id, doador.NomeCompleto, doador.Email, doador.Perfil);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var usuario = await usuarios.ObterPorEmailAsync(Usuario.NormalizarEmail(request.Email), ct);

        // Resposta idêntica para "e-mail não existe" e "senha errada":
        // não revelar quais e-mails estão cadastrados (LGPD/segurança).
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            return null;

        var (token, expiraEmUtc) = tokens.GerarToken(usuario);
        return new LoginResponse(token, expiraEmUtc, usuario.NomeCompleto, usuario.Perfil);
    }

    public async Task<MeResponse> ObterMeAsync(Guid usuarioId, CancellationToken ct)
    {
        var usuario = await usuarios.ObterPorIdAsync(usuarioId, ct)
            ?? throw new RecursoNaoEncontradoException("Usuário não encontrado.");

        var cpfMascarado = usuario.Perfil == PerfilUsuario.Doador && usuario.Cpf.Length == 11
            ? Cpf.Criar(usuario.Cpf).Mascarado()
            : null;

        return new MeResponse(
            usuario.Id, usuario.NomeCompleto, usuario.Email, usuario.Perfil, cpfMascarado);
    }
}
