namespace Doacoes.Api.Domain.Exceptions;

/// <summary>Dados da doação inválidos (HTTP 400).</summary>
public class DoacaoInvalidaException(string mensagem) : Exception(mensagem);

/// <summary>
/// Regra do edital: não se doa para campanhas encerradas ou canceladas (HTTP 422).
/// </summary>
public class CampanhaNaoDoavelException(string mensagem) : Exception(mensagem);

/// <summary>Campanha inexistente (HTTP 404).</summary>
public class CampanhaNaoEncontradaException(string mensagem) : Exception(mensagem);
