using FluentAssertions;
using ForHinanden.Api.Controllers;
using ForHinanden.Api.Models;
using ForHinanden.Tests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Task = System.Threading.Tasks.Task;

namespace Tests.Controllers;

public class RatingControllerTests
{
    [Fact]
    public async Task Create_Should_Return_BadRequest_If_Stars_Invalid()
    {
        // Arrange
        var context = TestDbContextFactory.CreateInMemory();
        var controller = new RatingController(context);

        var dto = new CreateRatingDto
        {
            TaskId = Guid.NewGuid(),
            ToUserId = "userB",
            RatedBy = "userA",
            Stars = 6
        };

        // Act
        var result = await controller.Create(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
}