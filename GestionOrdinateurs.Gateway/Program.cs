using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var keycloakAuthority = builder.Configuration["Authentication:Authority"];
if (!Uri.TryCreate(keycloakAuthority, UriKind.Absolute, out _))
{
    throw new InvalidOperationException(
        "Configure Authentication:Authority with the Keycloak realm URL.");
}
var tokenIssuer = builder.Configuration["Authentication:ValidIssuer"]
    ?? throw new InvalidOperationException("Configure Authentication:ValidIssuer with the Keycloak realm issuer.");
var tokenAudience = builder.Configuration["Authentication:Audience"]
    ?? throw new InvalidOperationException("Configure Authentication:Audience with the API audience.");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = tokenAudience;
        options.RequireHttpsMetadata = builder.Configuration.GetValue(
            "Authentication:RequireHttpsMetadata",
            !builder.Environment.IsDevelopment());
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = tokenIssuer,
            ValidateAudience = true,
            ValidAudience = tokenAudience,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5124", "https://localhost:7176")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy().RequireAuthorization();

app.Run();
