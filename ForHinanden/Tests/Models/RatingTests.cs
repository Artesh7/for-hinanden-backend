using Xunit;
using FluentAssertions;
using ForHinanden.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ForHinanden.Tests;

public class RatingTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Stars_Should_Be_Between_1_And_5(int stars)
    {
        // Arrange
        var rating = new Rating { Stars = stars, RatedBy = "user1", ToUserId = "user2", TaskId = Guid.NewGuid() };

        // Act
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(rating, new ValidationContext(rating), results, true);

        // Assert
        isValid.Should().BeFalse("Stars must be between 1 and 5");
    }

    [Fact]
    public void Stars_Within_Range_Should_Be_Valid()
    {
        var rating = new Rating { Stars = 4, RatedBy = "user1", ToUserId = "user2", TaskId = Guid.NewGuid() };
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(rating, new ValidationContext(rating), results, true);
        isValid.Should().BeTrue();
    }
}