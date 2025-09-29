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

        // ---- PRIORITY options ----
        await EnsurePriorityOptionAsync(db, "Snart");
        await EnsurePriorityOptionAsync(db, "Haster");
        await EnsurePriorityOptionAsync(db, "Fleksibel");

        // ---- DURATION options ----
        await EnsureDurationOptionAsync(db, "30 min");
        await EnsureDurationOptionAsync(db, "1 time");
        await EnsureDurationOptionAsync(db, "2 timer");
        await EnsureDurationOptionAsync(db, "3 timer");
        await EnsureDurationOptionAsync(db, "4 timer");
        await EnsureDurationOptionAsync(db, "5+ timer");

        // ---- CATEGORIES ----
        // 1) Rename gamle navne til de nye (bevarer relationer)
        await RenameCategoryIfExistsAsync(db, from: "Havearbejde", to: "Have & udendørs 🌱");
        await RenameCategoryIfExistsAsync(db, from: "Dyr",          to: "Dyr & kæledyr 🐶");

        // 2) Sørg for at alle de nye kategorier findes
        string[] desiredCategories =
        {
            "Praktisk hjælp i hjemmet 🏠",
            "Have & udendørs 🌱",
            "Flytning & transport 🚚",
            "Dyr & kæledyr 🐶",
            "Sundhed & omsorg ❤️",
            "Socialt & fællesskab 🤝",
            "Andet … ✨"
        };
        foreach (var name in desiredCategories)
            await EnsureCategoryAsync(db, name);

        await db.SaveChangesAsync();

        // ---- Backfill FK defaults på ældre tasks (bevarer din eksisterende logik) ----
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

    // === Helpers ===

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

    /// <summary>
    /// Omdøb en eksisterende kategori til et nyt navn. Hvis mål-navnet allerede findes,
    /// re-mapper vi TaskCategories fra den gamle til den eksisterende og sletter den gamle.
    /// </summary>
    private static async Task RenameCategoryIfExistsAsync(AppDbContext db, string from, string to)
    {
        var old = await db.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == from.Trim().ToLower());
        if (old is null) return;

        var target = await db.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == to.Trim().ToLower());
        if (target is null)
        {
            old.Name = to.Trim();
            await db.SaveChangesAsync();
            return;
        }

        // Mål findes allerede -> remap alle TaskCategories og slet den gamle
        await db.Database.ExecuteSqlRawAsync(@"
UPDATE ""TaskCategories""
SET ""CategoryId"" = {0}
WHERE ""CategoryId"" = {1};
", target.Id, old.Id);

        db.Categories.Remove(old);
        await db.SaveChangesAsync();
    }
}
