using System;
using System.Threading.Tasks;
using ForHinanden.Api.Controllers;
using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Test.TestDbContextFactory;
using Xunit;
using Assert = Xunit.Assert;
using Task = System.Threading.Tasks.Task;

namespace Test_backend.Controllers
{
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
            var result = await controller.Create(dto, null);
            var obj = result as ObjectResult;

            // Assert
            Assert.Equal(403, obj!.StatusCode);
        }
        
        [Fact]
        public async Task Create_Should_Return_400_If_Content_Empty()
        {
            // Arrange
            var context = TestDbContextFactory.CreateInMemory();
            var controller = new MessageController(context);

            var task = new ForHinanden.Api.Models.Task
            {
                Id = Guid.NewGuid(),
                RequestedBy = "user1",
                IsAccepted = true
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var dto = new CreateMessageDto
            {
                TaskId = task.Id,
                Sender = "user1",
                Receiver = "user2",
                Content = "" // Empty content
            };

            // Act
            var result = await controller.Create(dto, null);
            var obj = result as ObjectResult;

            // Assert
            Assert.Equal(400, obj!.StatusCode);
        }
    }
    
}