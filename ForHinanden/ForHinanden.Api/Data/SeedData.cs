using System;
using System.Linq;
using System.Threading.Tasks;
using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace ForHinanden.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // ---- Cities (by name) ----
        await EnsureCityAsync(db, "Vejle");
        await EnsureCityAsync(db, "Horsens");

        // ---- Categories (by name) ----
        await EnsureCategoryAsync(db, "Havearbejde");
        await EnsureCategoryAsync(db, "Dyr");

        // ---- Priority options (manual, by name) ----
        await EnsurePriorityOptionAsync(db, "Snart");
        await EnsurePriorityOptionAsync(db, "Haster");
        await EnsurePriorityOptionAsync(db, "Fleksibel");

        // ---- Duration options (manual, by name) ----
        await EnsureDurationOptionAsync(db, "30 min");
        await EnsureDurationOptionAsync(db, "1 time");
        await EnsureDurationOptionAsync(db, "2 timer");
        await EnsureDurationOptionAsync(db, "3 timer");
        await EnsureDurationOptionAsync(db, "4 timer");
        await EnsureDurationOptionAsync(db, "5+ timer");

        await db.SaveChangesAsync();

        // ---- Backfill CityId on existing tasks to a safe default (Vejle) ----
        var defaultCityId = await db.Cities
            .Where(c => c.Name.ToLower() == "vejle")
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (defaultCityId != Guid.Empty)
        {
            await db.Database.ExecuteSqlRawAsync(@"
UPDATE ""Tasks""
SET ""CityId"" = {0}
WHERE ""CityId"" IS NULL
   OR ""CityId"" = '00000000-0000-0000-0000-000000000000'::uuid;
", defaultCityId);
        }

        // Backfill FK defaults (only if you want a non-null default everywhere)
        var defaultPriorityId = await db.PriorityOptions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        if (defaultPriorityId != Guid.Empty)
        {
            await db.Database.ExecuteSqlRawAsync(@"
UPDATE ""Tasks""
SET ""PriorityOptionId"" = {0}
WHERE ""PriorityOptionId"" IS NULL
   OR ""PriorityOptionId"" = '00000000-0000-0000-0000-000000000000'::uuid;
", defaultPriorityId);
        }

        var defaultDurationId = await db.DurationOptions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => d.Id)
            .FirstOrDefaultAsync();

        if (defaultDurationId != Guid.Empty)
        {
            await db.Database.ExecuteSqlRawAsync(@"
UPDATE ""Tasks""
SET ""DurationOptionId"" = {0}
WHERE ""DurationOptionId"" IS NULL
   OR ""DurationOptionId"" = '00000000-0000-0000-0000-000000000000'::uuid;
", defaultDurationId);
        }
    }

    // Helpers — compare by TRIM + case-insensitive name
    private static async Task EnsureCityAsync(AppDbContext db, string name)
    {
        var n = name.Trim();
        var exists = await db.Cities.AnyAsync(c => c.Name.ToLower() == n.ToLower());
        if (!exists) db.Cities.Add(new City { Name = n });
    }

    private static async Task EnsureCategoryAsync(AppDbContext db, string name)
    {
        var n = name.Trim();
        var exists = await db.Categories.AnyAsync(c => c.Name.ToLower() == n.ToLower());
        if (!exists) db.Categories.Add(new Category { Name = n });
    }

    private static async Task EnsurePriorityOptionAsync(AppDbContext db, string name)
    {
        var n = name.Trim();
        var exists = await db.PriorityOptions.AnyAsync(p => p.Name.ToLower() == n.ToLower());
        if (!exists) db.PriorityOptions.Add(new PriorityOption { Name = n, IsActive = true });
    }

    private static async Task EnsureDurationOptionAsync(AppDbContext db, string name)
    {
        var n = name.Trim();
        var exists = await db.DurationOptions.AnyAsync(d => d.Name.ToLower() == n.ToLower());
        if (!exists) db.DurationOptions.Add(new DurationOption { Name = n, IsActive = true });
    }
}
