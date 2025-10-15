using FluentAssertions;
using ForHinanden.Api.Controllers;
using ForHinanden.Api.Models;
using ForHinanden.Tests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Task = System.Threading.Tasks.Task;

namespace Tests.Controllers;

public class MessageControllerTests
{
    [Fact]
    public async Task Create_Should_Return_403_If_Task_Not_Accepted()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemory();
        var controller = new MessageController(context);

        var task = new ForHinanden.Api.Models.Task
        {
            Id = Guid.NewGuid(),
            Title = "Test task",
            RequestedBy = "user1",
            IsAccepted = false
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var dto = new CreateMessageDto
        {
            TaskId = task.Id,
            Sender = "user1",
            Receiver = "user2",
            Content = "Hej!"
        };

        // Act
        var result = await controller.Create(dto);

        // Assert
        var obj = result as ObjectResult;
        obj!.StatusCode.Should().Be(403);
    }
}