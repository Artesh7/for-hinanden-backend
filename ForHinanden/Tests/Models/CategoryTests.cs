using Xunit;
using FluentAssertions;
using ForHinanden.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ForHinanden.Tests;

public class CategoryTests
{
    [Fact]
    public void Category_Should_Have_Required_Name()
    {
        // Arrange
        var category = new Category(); // Name = null!

        // Act
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(
            category,
            new ValidationContext(category),
            validationResults,
            true
        );

        // Assert
        isValid.Should().BeFalse(); // Model should be invalid
        validationResults.Should().Contain(v => v.MemberNames.Contains("Name"));
    }
}