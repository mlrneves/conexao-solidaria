namespace Campanhas.Domain.Exceptions;

/// <summary>
/// Violação de regra de negócio do domínio (mapeada para HTTP 400 na API).
/// </summary>
public class RegraDeNegocioException(string mensagem) : Exception(mensagem);

/// <summary>
/// Conflito com estado já existente, ex.: e-mail duplicado (HTTP 409).
/// </summary>
public class ConflitoException(string mensagem) : Exception(mensagem);

/// <summary>
/// Recurso inexistente (HTTP 404).
/// </summary>
public class RecursoNaoEncontradoException(string mensagem) : Exception(mensagem);
