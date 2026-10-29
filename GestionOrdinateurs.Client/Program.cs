using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.SessionStorage;
using GestionOrdinateurs.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var authenticationApiBaseAddress = builder.Configuration["AuthenticationApi:BaseAddress"];
if (!Uri.TryCreate(authenticationApiBaseAddress, UriKind.Absolute, out var authenticationApiUri))
{
    throw new InvalidOperationException("Configure AuthenticationApi:BaseAddress in wwwroot/appsettings.json.");
}

builder.Services.AddHttpClient("AuthenticationApi", client => client.BaseAddress = authenticationApiUri);
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddTransient<SessionTokenHandler>();
var gatewayBaseAddress = builder.Configuration["GestionOrdinateursGateway:BaseAddress"];
if (!Uri.TryCreate(gatewayBaseAddress, UriKind.Absolute, out var gatewayUri))
{
    throw new InvalidOperationException("Configure GestionOrdinateursGateway:BaseAddress in wwwroot/appsettings.json.");
}

builder.Services.AddHttpClient("GestionOrdinateursGateway", client =>
{
    client.BaseAddress = gatewayUri;
}).AddHttpMessageHandler<SessionTokenHandler>();

await builder.Build().RunAsync();
