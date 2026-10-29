using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using GestionOrdinateurs.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var keycloakAuthority = builder.Configuration["Keycloak:Authority"];
if (!Uri.TryCreate(keycloakAuthority, UriKind.Absolute, out _))
{
    throw new InvalidOperationException("Configure Keycloak:Authority in wwwroot/appsettings.json.");
}

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = keycloakAuthority;
    options.ProviderOptions.ClientId = builder.Configuration["Keycloak:ClientId"]
        ?? throw new InvalidOperationException("Configure Keycloak:ClientId in wwwroot/appsettings.json.");
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Clear();
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("api-access");
    options.UserOptions.NameClaim = "preferred_username";
    options.UserOptions.RoleClaim = "roles";
});

var gatewayBaseAddress = builder.Configuration["GestionOrdinateursGateway:BaseAddress"];
if (!Uri.TryCreate(gatewayBaseAddress, UriKind.Absolute, out var gatewayUri))
{
    throw new InvalidOperationException("Configure GestionOrdinateursGateway:BaseAddress in wwwroot/appsettings.json.");
}

builder.Services.AddHttpClient("GestionOrdinateursGateway", client =>
{
    client.BaseAddress = gatewayUri;
}).AddHttpMessageHandler(serviceProvider =>
    serviceProvider.GetRequiredService<AuthorizationMessageHandler>()
        .ConfigureHandler(authorizedUrls: [gatewayUri.ToString()]));

await builder.Build().RunAsync();
