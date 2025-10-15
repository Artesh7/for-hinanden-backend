using FluentAssertions;
using ForHinanden.Api.Models;

namespace ForHinanden.Tests.TestUtilities;

public class TestDbContextFactoryTests
{
    [Fact]
    public void Should_Create_InMemory_Database_And_Save_Data()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemory();

        // Act
        context.Categories.Add(new Category { Name = "Test kategori" });
        context.SaveChanges();

        // Assert
        var count = context.Categories.Count();
        count.Should().Be(1);
    }

}