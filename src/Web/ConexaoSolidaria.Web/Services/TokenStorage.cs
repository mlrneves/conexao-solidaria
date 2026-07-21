using Microsoft.JSInterop;

namespace ConexaoSolidaria.Web.Services;

/// <summary>
/// Guarda o JWT no sessionStorage via JS interop (módulo Blazor do curso).
/// sessionStorage em vez de localStorage: o token morre ao fechar a aba —
/// superfície menor de exposição. Em produção o padrão seria um BFF com
/// cookie HttpOnly (ver docs/LGPD.md).
/// </summary>
public class TokenStorage(IJSRuntime js)
{
    private const string Chave = "conexao_token";

    public ValueTask<string?> ObterAsync()
        => js.InvokeAsync<string?>("sessionStorage.getItem", Chave);

    public ValueTask GuardarAsync(string token)
        => js.InvokeVoidAsync("sessionStorage.setItem", Chave, token);

    public ValueTask RemoverAsync()
        => js.InvokeVoidAsync("sessionStorage.removeItem", Chave);
}
