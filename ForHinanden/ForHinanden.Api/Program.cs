using ForHinanden.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;   // 👈 vigtig for Configure<JsonOptions>

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();

// Global JSON-konfiguration (enums som strings)
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DB: Postgres via ENV (DATABASE_URL) ---
var connStr = Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? builder.Configuration.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("DATABASE_URL/DefaultConnection mangler.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStr));

var app = builder.Build();

// --- Migrér DB på opstart ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// --- Swagger (også i prod) ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Lyt på Render's PORT (fallback 5010 lokalt)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5010";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();