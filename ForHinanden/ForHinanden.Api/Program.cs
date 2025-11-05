using ForHinanden.Api.Data;
using ForHinanden.Api.Hubs;               // SignalR hub
using ForHinanden.Api.Services;          // ICloudinaryService + implementations
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
    ?? string.Empty; // allow empty when using InMemory in Development

// Use InMemory in Development to simplify local runs without Postgres
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("ForHinandenDev"));
    Console.WriteLine("Using InMemory database for Development.");
}
else
{
    if (string.IsNullOrWhiteSpace(connStr))
        throw new InvalidOperationException("DATABASE_URL/DefaultConnection mangler.");

    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStr));
}

// ---------------- SignalR ----------------
builder.Services.AddSignalR();

// Register chat activity registry (used by Hub + controllers)
builder.Services.AddSingleton<ActiveChatRegistry>();

// ---------------- Cloudinary ----------------
// Register a development-friendly no-op implementation when CLOUDINARY_URL is missing
var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
if (string.IsNullOrWhiteSpace(cloudinaryUrl))
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: register a no-op service so the app can run without real Cloudinary credentials
        builder.Services.AddSingleton<ICloudinaryService, NoOpCloudinaryService>();
        Console.WriteLine("CLOUDINARY_URL not set — registered NoOpCloudinaryService for Development.");
    }
    else
    {
        throw new InvalidOperationException("CLOUDINARY_URL er ikke konfigureret.");
    }
}
else
{
    // Production / provided: register real wrapper
    builder.Services.AddSingleton<ICloudinaryService>(sp => new CloudinaryWrapper(new Cloudinary(cloudinaryUrl)));
}

// ---------------- Firebase Admin SDK ----------------// Read the env var
var firebaseKeyJson = Environment.GetEnvironmentVariable("FIREBASE_KEY_JSON");
if (string.IsNullOrWhiteSpace(firebaseKeyJson))
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: skip firebase initialization
        Console.WriteLine("FIREBASE_KEY_JSON not set — skipping Firebase initialization in Development.");
    }
    else
    {
        throw new InvalidOperationException("FIREBASE_KEY_JSON is not set.");
    }
}
else
{
    // Initialize Firebase
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(firebaseKeyJson)
    });
}

// ---------------- Background jobs ----------------
// Twice-daily task digests per user/city
builder.Services.AddHostedService<ForHinanden.Api.Services.TaskDigestService>();

// ---------------- Build app ----------------
var app = builder.Build();

// Migrate DB + seed on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    if (!builder.Environment.IsDevelopment())
    {
        db.Database.Migrate();
    }

    await SeedData.EnsureSeededAsync(services);
}

// ---------------- Middleware ----------------
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
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