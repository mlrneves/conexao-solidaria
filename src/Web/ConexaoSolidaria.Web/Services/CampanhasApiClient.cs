using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConexaoSolidaria.Web.Models;

namespace ConexaoSolidaria.Web.Services;

/// <summary>Typed client do microsserviço Campanhas.Api.</summary>
public class CampanhasApiClient(HttpClient http)
{
    public async Task<List<CampanhaPublica>> PainelPublicoAsync()
        => await http.GetFromJsonAsync<List<CampanhaPublica>>("api/publico/campanhas") ?? [];

    public async Task<LoginResult?> LoginAsync(LoginForm form)
    {
        using var resposta = await http.PostAsJsonAsync("api/auth/login", form);
        if (!resposta.IsSuccessStatusCode) return null;
        return await resposta.Content.ReadFromJsonAsync<LoginResult>();
    }

    public async Task<string?> RegistrarAsync(CadastroForm form)
    {
        using var resposta = await http.PostAsJsonAsync("api/auth/registrar", new
        {
            form.NomeCompleto,
            form.Email,
            form.Cpf,
            form.Senha,
            form.ConsentimentoLgpd
        });
        return resposta.IsSuccessStatusCode ? null : await LerErroAsync(resposta);
    }

    public async Task<Me?> MeAsync()
    {
        using var resposta = await http.GetAsync("api/auth/me");
        if (!resposta.IsSuccessStatusCode) return null;
        return await resposta.Content.ReadFromJsonAsync<Me>();
    }

    public async Task<List<CampanhaGestor>> ListarTodasAsync()
        => await http.GetFromJsonAsync<List<CampanhaGestor>>("api/campanhas") ?? [];

    public async Task<string?> CriarCampanhaAsync(CampanhaForm form)
    {
        using var resposta = await http.PostAsJsonAsync("api/campanhas", CorpoCampanha(form));
        return resposta.IsSuccessStatusCode ? null : await LerErroAsync(resposta);
    }

    public async Task<string?> AtualizarCampanhaAsync(CampanhaForm form)
    {
        using var resposta = await http.PutAsJsonAsync(
            $"api/campanhas/{form.Id}", CorpoCampanha(form, incluirStatus: true));
        return resposta.IsSuccessStatusCode ? null : await LerErroAsync(resposta);
    }

    private static object CorpoCampanha(CampanhaForm form, bool incluirStatus = false)
        => incluirStatus
            ? new
            {
                form.Titulo,
                form.Descricao,
                DataInicio = DateTime.SpecifyKind(form.DataInicio, DateTimeKind.Utc),
                DataFim = DateTime.SpecifyKind(form.DataFim, DateTimeKind.Utc),
                form.MetaFinanceira,
                form.Status
            }
            : new
            {
                form.Titulo,
                form.Descricao,
                DataInicio = DateTime.SpecifyKind(form.DataInicio, DateTimeKind.Utc),
                DataFim = DateTime.SpecifyKind(form.DataFim, DateTimeKind.Utc),
                form.MetaFinanceira
            } as object;

    /// <summary>Extrai a mensagem do ProblemDetails retornado pelas APIs.</summary>
    internal static async Task<string> LerErroAsync(HttpResponseMessage resposta)
    {
        try
        {
            using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            if (json.RootElement.TryGetProperty("detail", out var detail))
                return detail.GetString() ?? resposta.ReasonPhrase ?? "Erro inesperado.";
            if (json.RootElement.TryGetProperty("errors", out var errors))
            {
                var primeira = errors.EnumerateObject().FirstOrDefault();
                if (primeira.Value.ValueKind == JsonValueKind.Array && primeira.Value.GetArrayLength() > 0)
                    return primeira.Value[0].GetString() ?? "Dados inválidos.";
            }
            if (json.RootElement.TryGetProperty("title", out var title))
                return title.GetString() ?? "Erro inesperado.";
        }
        catch
        {
            // corpo não-JSON
        }

        return resposta.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Sessão expirada — faça login novamente.",
            HttpStatusCode.Forbidden => "Você não tem permissão para esta operação.",
            _ => "Erro inesperado ao chamar a API."
        };
    }
}
