using Xunit;
using FluentAssertions;
using ForHinanden.Api.Models;
using System;

namespace ForHinanden.Tests;

public class TaskTests
{
    [Fact]
    public void CreatedAt_Should_Be_Set_Automatically()
    {
        // Arrange
        var task = new ForHinanden.Api.Models.Task();

        // Act
        var now = DateTime.UtcNow;
        var createdAt = task.CreatedAt;

        // Assert
        createdAt.Should().BeCloseTo(now, precision: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Task_Should_Not_Be_Accepted_By_Default()
    {
        var task = new ForHinanden.Api.Models.Task();
        task.IsAccepted.Should().BeFalse();
    }
}