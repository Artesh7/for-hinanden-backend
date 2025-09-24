using System;
using ForHinanden.Api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DB: Postgres via ENV (DATABASE_URL) ---
var connStr = Environment.GetEnvironmentVariable("DATABASE_URL")
              ?? builder.Configuration.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("DATABASE_URL/DefaultConnection mangler.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connStr));

var app = builder.Build();

// --- Migrér DB på opstart ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// --- Swagger i Prod også, så du kan teste online ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Lyt på Render's PORT (fallback 5010 lokalt)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5010";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();