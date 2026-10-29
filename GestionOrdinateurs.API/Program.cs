using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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

builder.Services.AddControllers();
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<ReparationService>();
builder.Services.AddSingleton<StockService>();
builder.Services.AddSingleton<JournalService>();
builder.Services.AddDbContextFactory<ParcInformatiqueContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("ParcInfo") ?? "Data Source=parcinfo.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5124", "https://localhost:7176")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Saisissez le token JWT."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();
    context.Database.Migrate();

    if (!context.Ordinateurs.Any())
    {
        context.Ordinateurs.AddRange(ParcInitializer.InitialiserParc());
        await context.SaveChangesAsync();
    }

    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new User { Login = "user1", Role = RoleEnum.Admin },
            new User { Login = "user2", Role = RoleEnum.Gestionnaire },
            new User { Login = "user3", Role = RoleEnum.Gestionnaire });
        await context.SaveChangesAsync();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
