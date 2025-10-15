using ForHinanden.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ForHinanden.Tests.TestUtilities;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // ny “database” pr. test
            .Options;

        return new AppDbContext(options);
    }
}