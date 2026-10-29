using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Configure Jwt:Key with a signing key of at least 32 bytes using user secrets or another secure configuration provider.");
}

var usersApiBaseAddress = builder.Configuration["UsersApi:BaseAddress"];
if (!Uri.TryCreate(usersApiBaseAddress, UriKind.Absolute, out var usersApiUri))
{
    throw new InvalidOperationException("Configure UsersApi:BaseAddress with the GestionOrdinateurs.API base URL.");
}

builder.Services.AddControllers();
builder.Services.AddHttpClient("UsersApi", client => client.BaseAddress = usersApiUri);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5124", "https://localhost:7176")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowBlazorClient");
app.MapControllers();

app.Run();
