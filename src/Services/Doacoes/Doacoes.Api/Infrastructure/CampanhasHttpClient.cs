using System.Net;

namespace Doacoes.Api.Infrastructure;

/// <summary>
/// Typed client HTTP para o Campanhas.Api. A resiliência (retry, circuit
/// breaker, timeout) é aplicada no pipeline do HttpClient via
/// AddStandardResilienceHandler no Program.cs.
/// </summary>
public class CampanhasHttpClient(HttpClient http) : ICampanhasClient
{
    public async Task<CampanhaStatusDto?> ObterCampanhaAsync(Guid id, CancellationToken ct = default)
    {
        using var resposta = await http.GetAsync($"api/publico/campanhas/{id}", ct);

        if (resposta.StatusCode == HttpStatusCode.NotFound)
            return null;

        resposta.EnsureSuccessStatusCode();
        return await resposta.Content.ReadFromJsonAsync<CampanhaStatusDto>(cancellationToken: ct);
    }
}
