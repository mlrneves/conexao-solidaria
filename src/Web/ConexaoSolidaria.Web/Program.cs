using ConexaoSolidaria.Web;
using ConexaoSolidaria.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// URLs das APIs vêm de wwwroot/appsettings.json (substituído por ambiente:
// dev 5081/5082, compose 8081/8082, Kubernetes 30081/30082 via ConfigMap).
var campanhasBaseUrl = builder.Configuration["Api:CampanhasBaseUrl"] ?? "http://localhost:5081";
var doacoesBaseUrl = builder.Configuration["Api:DoacoesBaseUrl"] ?? "http://localhost:5082";

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenStorage>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddTransient<AuthMessageHandler>();

builder.Services.AddHttpClient<CampanhasApiClient>(c => c.BaseAddress = new Uri(campanhasBaseUrl))
    .AddHttpMessageHandler<AuthMessageHandler>();
builder.Services.AddHttpClient<DoacoesApiClient>(c => c.BaseAddress = new Uri(doacoesBaseUrl))
    .AddHttpMessageHandler<AuthMessageHandler>();

await builder.Build().RunAsync();
