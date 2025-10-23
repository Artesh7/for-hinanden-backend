using ForHinanden.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Test.TestDbContextFactory;

public class TestDbContextFactory
{
    public static AppDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}