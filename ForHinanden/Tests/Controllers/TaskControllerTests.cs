

using FluentAssertions;
using ForHinanden.Api.Controllers;
using ForHinanden.Api.Hubs;
using ForHinanden.Api.Models;
using ForHinanden.Tests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Task = System.Threading.Tasks.Task;

public class TaskControllerTests
{
    [Fact]
    public async Task GetAll_Should_Return_All_Tasks()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemory();
        var fakeHub = new Mock<IHubContext<NotificationsHub>>();
        var controller = new TaskController(context, fakeHub.Object);

        var city = new City { Name = "Aarhus" };
        var duration = new DurationOption { Name = "1 time" };
        var priority = new PriorityOption { Name = "Normal" };
        var category = new Category { Name = "Hjælp" };

        context.AddRange(city, duration, priority, category);
        context.SaveChanges();

        var task = new ForHinanden.Api.Models.Task
        {
            Title = "Handle ind",
            Description = "Køb mælk",
            RequestedBy = "user1",
            City = city,
            DurationOption = duration,
            PriorityOption = priority,
            TaskCategories = new List<TaskCategory> { new() { Category = category } }
        };

        context.Tasks.Add(task);
        context.SaveChanges();

        // Act
        var result = await controller.GetAll();
        var okResult = result as OkObjectResult;

        // Assert
        okResult.Should().NotBeNull();
        okResult!.Value.Should().NotBeNull();
    }    
}