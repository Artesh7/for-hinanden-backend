using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;
using TaskEntity = ForHinanden.Api.Models.Task;

namespace ForHinanden.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<User> Users => Set<User>();           // ⬅️ User med deviceId som nøgle
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<TaskOffer> TaskOffers => Set<TaskOffer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map List<string> til Postgres text[]
        modelBuilder.Entity<TaskEntity>()
            .Property(t => t.Categories)
            .HasColumnType("text[]");

        // Én offer pr. (TaskId, OfferedBy)
        modelBuilder.Entity<TaskOffer>()
            .HasIndex(o => new { o.TaskId, o.OfferedBy })
            .IsUnique();
    }
}