using Xunit;
using FluentAssertions;
using ForHinanden.Api.Data;
using Microsoft.EntityFrameworkCore;
using ForHinanden.Api.Models;

namespace ForHinanden.Tests;

public class CategoryDatabaseTests
{
    [Fact]
    public void Can_Add_And_Retrieve_Category_From_InMemory_Database()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using (var context = new AppDbContext(options))
        {
            var category = new Category { Name = "Hjælp i hjemmet" };
            context.Categories.Add(category);
            context.SaveChanges();
        }

        using (var context = new AppDbContext(options))
        {
            var categories = context.Categories.ToList();
            categories.Should().ContainSingle();
            categories.First().Name.Should().Be("Hjælp i hjemmet");
        }
    }
}