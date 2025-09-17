using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;
using TaskEntity = ForHinanden.Api.Models.Task;

namespace ForHinanden.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // NY DbSet – brug denne i controlleren
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Message> Messages => Set<Message>();
}