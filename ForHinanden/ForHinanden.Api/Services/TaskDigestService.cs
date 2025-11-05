using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using ForHinanden.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ForHinanden.Api.Services;

/// <summary>
/// Sends twice-daily digests of new tasks per user (by city), excluding the user's own tasks.
/// Runs at 09:00 and 18:00 server local time. In Development with InMemory DB, it's best-effort.
/// </summary>
public class TaskDigestService : BackgroundService
{
    private readonly IServiceProvider _services;

    // Track last window we processed to avoid double-sending after restarts within same window
    private DateTime _lastWindowStartUtc = DateTime.UtcNow.AddHours(-12);

    public TaskDigestService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Simple scheduler: wake up every 15 minutes and check if current time is close to target slots
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nowLocal = DateTime.Now; // server local
                var shouldRun = IsWithinRunWindow(nowLocal, 9, 0) || IsWithinRunWindow(nowLocal, 18, 0);

                if (shouldRun)
                {
                    await SendDigestsAsync(stoppingToken);

                    // After a run, wait ~1 hour to avoid repeating in same window
                    await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
                    continue;
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"TaskDigestService error: {ex}");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
            catch (TaskCanceledException) { }
        }
    }

    private static bool IsWithinRunWindow(DateTime nowLocal, int hour, int minute)
    {
        var target = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, hour, minute, 0);
        var diff = nowLocal - target;
        return diff.TotalMinutes >= 0 && diff.TotalMinutes <= 10; // 10-minute window
    }

    private async Task SendDigestsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Window: since last run until now (UTC)
        var nowUtc = DateTime.UtcNow;
        var windowStartUtc = _lastWindowStartUtc;
        var windowEndUtc = nowUtc;

        // Collect tasks created in window, include City
        var tasksInWindow = await db.Tasks
            .AsNoTracking()
            .Include(t => t.City)
            .Where(t => t.CreatedAt >= windowStartUtc && t.CreatedAt < windowEndUtc)
            .ToListAsync(ct);

        if (tasksInWindow.Count == 0)
        {
            _lastWindowStartUtc = windowEndUtc;
            return;
        }

        // Pull all users to compute per-user counts by their City string
        var users = await db.Users.AsNoTracking().ToListAsync(ct);

        foreach (var user in users)
        {
            if (string.IsNullOrWhiteSpace(user.DeviceId)) continue; // need token
            if (string.IsNullOrWhiteSpace(user.City)) continue;     // can't determine area

            // Consider tasks in same city as user's City string
            var cityTasks = tasksInWindow.Where(t => t.City != null && string.Equals(t.City.Name, user.City, StringComparison.OrdinalIgnoreCase));

            // Exclude tasks created by the user themselves
            var count = cityTasks.Count(t => !string.Equals(t.RequestedBy ?? string.Empty, user.DeviceId, StringComparison.OrdinalIgnoreCase));

            if (count <= 0) continue;

            var title = count == 1
                ? "En person i nærheden har brug for hjælp!"
                : $"{count} personer i nærheden har brug for hjælp!";

            var message = new Message
            {
                Token = user.DeviceId,
                Notification = new Notification
                {
                    Title = title,
                    Body = "Tryk for at se opgaver i dit område."
                },
                Data = new Dictionary<string, string>
                {
                    { "type", "task_digest" },
                    { "count", count.ToString() },
                    { "route", "/feed" }
                }
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TaskDigest FCM send failed for {user.DeviceId}: {ex.Message}");
            }
        }

        // Advance window
        _lastWindowStartUtc = windowEndUtc;
    }
}

