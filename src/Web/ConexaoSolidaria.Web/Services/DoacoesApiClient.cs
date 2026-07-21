using System.Net.Http.Json;
using ConexaoSolidaria.Web.Models;

namespace ConexaoSolidaria.Web.Services;

/// <summary>Typed client do microsserviço Doacoes.Api.</summary>
public class DoacoesApiClient(HttpClient http)
{
    /// <summary>Retorna (resposta 202, null) no sucesso ou (null, mensagem) no erro.</summary>
    public async Task<(DoacaoAceita? Aceita, string? Erro)> DoarAsync(Guid campanhaId, decimal valor)
    {
        using var resposta = await http.PostAsJsonAsync("api/doacoes", new { campanhaId, valor });

        if (!resposta.IsSuccessStatusCode)
            return (null, await CampanhasApiClient.LerErroAsync(resposta));

        var aceita = await resposta.Content.ReadFromJsonAsync<DoacaoAceita>();
        return (aceita, null);
    }

    public async Task<List<MinhaDoacao>> MinhasAsync()
        => await http.GetFromJsonAsync<List<MinhaDoacao>>("api/doacoes/minhas") ?? [];
}
