using ForHinanden.Api.Data;
using ForHinanden.Api.Hubs;               // SignalR hub
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloudinaryDotNet;                   // Cloudinary
using FirebaseAdmin;                      // Firebase Admin SDK
using Google.Apis.Auth.OAuth2;            // Google Credentials

var builder = WebApplication.CreateBuilder(args);

// ---------------- Controllers & Swagger ----------------
builder.Services.AddControllers();

// Global JSON (camelCase + enums as strings)
builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Allow up to 10 MB uploads (optional)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------------- Database ----------------
var connStr =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL/DefaultConnection mangler.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStr));

// ---------------- SignalR ----------------
builder.Services.AddSignalR();

// ---------------- Cloudinary ----------------
builder.Services.AddSingleton(sp =>
{
    var url = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
    if (string.IsNullOrWhiteSpace(url))
        throw new InvalidOperationException("CLOUDINARY_URL er ikke konfigureret.");
    var cld = new Cloudinary(url);
    cld.Api.Secure = true;
    return cld;
});

// ---------------- Firebase Admin SDK ----------------// Read the env var
var firebaseKeyJson = Environment.GetEnvironmentVariable("FIREBASE_KEY_JSON");
if (string.IsNullOrWhiteSpace(firebaseKeyJson))
    throw new InvalidOperationException("FIREBASE_KEY_JSON is not set.");

// Initialize Firebase
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromJson(firebaseKeyJson)
});

// ---------------- Build app ----------------
var app = builder.Build();

// Migrate DB + seed on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedData.EnsureSeededAsync(services);
}

// ---------------- Middleware ----------------
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// SignalR hub endpoint
app.MapHub<NotificationsHub>("/hubs/notifications");

// ---------------- Hosting (Render) ----------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "5010";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();