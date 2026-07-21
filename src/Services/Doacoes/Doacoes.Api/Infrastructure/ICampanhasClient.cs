namespace Doacoes.Api.Infrastructure;

public record CampanhaStatusDto(Guid Id, string Titulo, string Status)
{
    public bool EstaAtiva => Status.Equals("Ativa", StringComparison.OrdinalIgnoreCase);
}

// SOLID (ISP/DIP): um único método — exatamente o que o fluxo de doação
// precisa saber do microsserviço de Campanhas.
public interface ICampanhasClient
{
    /// <summary>Retorna null quando a campanha não existe (404 no serviço de Campanhas).</summary>
    Task<CampanhaStatusDto?> ObterCampanhaAsync(Guid id, CancellationToken ct = default);
}
