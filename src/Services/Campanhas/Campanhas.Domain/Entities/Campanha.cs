using Campanhas.Domain.Enums;
using Campanhas.Domain.Exceptions;

namespace Campanhas.Domain.Entities;

public class Campanha
{
    public Guid Id { get; private set; }
    public string Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public decimal MetaFinanceira { get; private set; }
    public StatusCampanha Status { get; private set; }
    public decimal ValorTotalArrecadado { get; private set; }
    public Guid CriadoPorUsuarioId { get; private set; }
    public DateTime CriadoEmUtc { get; private set; }

    private Campanha() { } // EF Core

    // SOLID (SRP/OCP): as regras de criação moram na própria entidade — novas
    // validações entram aqui, sem alterar controllers ou serviços de aplicação.
    public static Campanha Criar(
        string titulo, string descricao, DateTime dataInicio, DateTime dataFim,
        decimal metaFinanceira, Guid criadoPorUsuarioId, DateTime agoraUtc)
    {
        ValidarDadosBasicos(titulo, descricao, dataInicio, dataFim, metaFinanceira);

        // Regra do edital: campanha não pode ser criada com data de término no passado.
        if (dataFim < agoraUtc)
            throw new RegraDeNegocioException(
                "A data de término da campanha não pode estar no passado.");

        return new Campanha
        {
            Id = Guid.NewGuid(),
            Titulo = titulo.Trim(),
            Descricao = descricao.Trim(),
            DataInicio = dataInicio,
            DataFim = dataFim,
            MetaFinanceira = metaFinanceira,
            Status = StatusCampanha.Ativa,
            ValorTotalArrecadado = 0,
            CriadoPorUsuarioId = criadoPorUsuarioId,
            CriadoEmUtc = agoraUtc
        };
    }

    public void Atualizar(
        string titulo, string descricao, DateTime dataInicio, DateTime dataFim,
        decimal metaFinanceira, StatusCampanha status)
    {
        ValidarDadosBasicos(titulo, descricao, dataInicio, dataFim, metaFinanceira);

        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        DataInicio = dataInicio;
        DataFim = dataFim;
        MetaFinanceira = metaFinanceira;
        Status = status;
    }

    private static void ValidarDadosBasicos(
        string titulo, string descricao, DateTime dataInicio, DateTime dataFim, decimal metaFinanceira)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new RegraDeNegocioException("Título é obrigatório.");
        if (titulo.Trim().Length > 150)
            throw new RegraDeNegocioException("Título deve ter no máximo 150 caracteres.");
        if (string.IsNullOrWhiteSpace(descricao))
            throw new RegraDeNegocioException("Descrição é obrigatória.");

        // Regra do edital: a meta financeira deve ser maior que zero.
        if (metaFinanceira <= 0)
            throw new RegraDeNegocioException("A meta financeira deve ser maior que zero.");

        if (dataFim <= dataInicio)
            throw new RegraDeNegocioException(
                "A data de término deve ser posterior à data de início.");
    }
}
