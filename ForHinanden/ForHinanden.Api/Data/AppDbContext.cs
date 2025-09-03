using ForHinanden.Api.Models;

using Microsoft.EntityFrameworkCore;


namespace ForHinanden.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<HelpRequest> HelpRequests => Set<HelpRequest>();
}
