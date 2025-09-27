using ForHinanden.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http.Features;
// +++
using ForHinanden.Api.Hubs; // SignalR hub
// +++

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();

// Global JSON (enums som strings)
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// (Valgfrit) tillad op til 10 MB uploads
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DB ---
var connStr = Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? builder.Configuration.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("DATABASE_URL/DefaultConnection mangler.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStr));

// +++ SignalR real-time +++
builder.Services.AddSignalR();
// +++

var app = builder.Build();

// --- Migrér DB på opstart ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedData.EnsureSeededAsync(services);
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Servér /wwwroot (billeder mm.)
app.UseStaticFiles();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// +++ Map SignalR hub (WebSocket endpoint) +++
app.MapHub<NotificationsHub>("/hubs/notifications");
// +++

// Lyt på Render's PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5010";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();