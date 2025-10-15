using Xunit;
using FluentAssertions;
using ForHinanden.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace ForHinanden.Tests;

public class UserTests
{
    [Fact]
    public void Bio_Should_Not_Exceed_500_Characters()
    {
        // Arrange
        var user = new User
        {
            DeviceId = "123",
            FirstName = "Omar",
            LastName = "Ali",
            City = "Aarhus",
            Bio = new string('x', 501)
        };

        // Act
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(user, new ValidationContext(user), results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains("Bio"));
    }

    [Fact]
    public void Valid_User_Should_Pass_Validation()
    {
        var user = new User
        {
            DeviceId = "456",
            FirstName = "Anna",
            LastName = "Hansen",
            City = "København",
            Bio = "Jeg elsker at hjælpe andre!"
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(user, new ValidationContext(user), results, true);

        isValid.Should().BeTrue();
    }
}