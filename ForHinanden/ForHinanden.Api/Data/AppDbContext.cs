using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;
using TaskEntity = ForHinanden.Api.Models.Task;

namespace ForHinanden.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<TaskOffer> TaskOffers => Set<TaskOffer>();
    public DbSet<HelpRelation> HelpRelations => Set<HelpRelation>(); // NEW

    // Lookups
    public DbSet<City> Cities => Set<City>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TaskCategory> TaskCategories => Set<TaskCategory>();

    // feedback
    public DbSet<Feedback> Feedbacks { get; set; }

    // NEW option tables
    public DbSet<PriorityOption> PriorityOptions => Set<PriorityOption>();
    public DbSet<DurationOption> DurationOptions => Set<DurationOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TaskOffers: one per (TaskId, OfferedBy)
        modelBuilder.Entity<TaskOffer>()
            .HasIndex(o => new { o.TaskId, o.OfferedBy })
            .IsUnique();

        // Task -> City (restrict delete)
        modelBuilder.Entity<TaskEntity>()
            .HasOne(t => t.City)
            .WithMany()
            .HasForeignKey(t => t.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskCategories join (GUID PK + unique pair)
        modelBuilder.Entity<TaskCategory>()
            .HasIndex(tc => new { tc.TaskId, tc.CategoryId })
            .IsUnique();

        modelBuilder.Entity<TaskCategory>()
            .HasOne(tc => tc.Task)
            .WithMany(t => t.TaskCategories)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskCategory>()
            .HasOne(tc => tc.Category)
            .WithMany()
            .HasForeignKey(tc => tc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique names
        modelBuilder.Entity<City>().HasIndex(c => c.Name).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        // NEW: unique names for option tables
        modelBuilder.Entity<PriorityOption>().HasIndex(p => p.Name).IsUnique();
        modelBuilder.Entity<DurationOption>().HasIndex(d => d.Name).IsUnique();

        // NEW: HelpRelation unique per task and undirected pair (UserA <= UserB)
        modelBuilder.Entity<HelpRelation>()
            .HasIndex(hr => new { hr.TaskId, hr.UserA, hr.UserB })
            .IsUnique();

        // Optional: fast lookup of peers for a user
        modelBuilder.Entity<HelpRelation>()
            .HasIndex(hr => new { hr.UserA, hr.UserB });
    }
}
