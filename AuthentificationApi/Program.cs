var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "http://localhost:5124", "https://localhost:7176")
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
