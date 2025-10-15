using Xunit;
using FluentAssertions;
using ForHinanden.Api.Models;
using System;

namespace ForHinanden.Tests;

public class MessageTests
{
    [Fact]
    public void SentAt_Should_Default_To_UtcNow()
    {
        // Arrange
        var message = new Message
        {
            TaskId = Guid.NewGuid(),
            Task = new ForHinanden.Api.Models.Task(),
            Sender = "user1",
            Receiver = "user2",
            Content = "Hej!"
        };

        // Act
        var now = DateTime.UtcNow;
        var sentAt = message.SentAt;

        // Assert
        sentAt.Should().BeCloseTo(now, precision: TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Message_Should_Contain_Sender_And_Receiver()
    {
        var message = new Message
        {
            TaskId = Guid.NewGuid(),
            Task = new ForHinanden.Api.Models.Task(),
            Sender = "A",
            Receiver = "B",
            Content = "Test besked"
        };

        message.Sender.Should().NotBeNullOrEmpty();
        message.Receiver.Should().NotBeNullOrEmpty();
    }
}