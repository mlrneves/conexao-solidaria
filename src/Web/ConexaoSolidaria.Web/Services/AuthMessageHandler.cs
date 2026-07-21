using System.Net.Http.Headers;

namespace ConexaoSolidaria.Web.Services;

/// <summary>Anexa o Bearer token (quando existir) a toda chamada às APIs.</summary>
public class AuthMessageHandler(TokenStorage tokenStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await tokenStorage.ObterAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
