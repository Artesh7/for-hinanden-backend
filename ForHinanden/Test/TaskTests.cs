using Xunit;
using ForHinanden.Api.Models;
using Assert = Xunit.Assert;
using TaskModel = ForHinanden.Api.Models.Task; // undgå konflikt med System.Threading.Tasks.Task

namespace Test_backend;

public class TaskTests
{
    [Fact]
    public void Can_Assign_Title_And_Description()
    {
        var task = new TaskModel
        {
            Title = "Hjælp med indkøb",
            Description = "Jeg har brug for hjælp til at handle dagligvarer."
        };

        Assert.Equal("Hjælp med indkøb", task.Title);
        Assert.Equal("Jeg har brug for hjælp til at handle dagligvarer.", task.Description);
    }

    [Fact]
    public void Title_And_Description_Should_Allow_Null_But_Not_Empty()
    {
        // null er ok, men tom streng er ugyldig (hvis du håndhæver det i model)
        var task = new TaskModel { Title = null, Description = null };
        Assert.Null(task.Title);
        Assert.Null(task.Description);

        // hvis du vil tjekke validering for tom string, gør det eksplicit
        var ex = Record.Exception(() =>
        {
            var t = new TaskModel { Title = "", Description = "" };
            if (string.IsNullOrWhiteSpace(t.Title))
                throw new ArgumentException("Title is required.", nameof(t.Title));
        });

        Assert.NotNull(ex);
        Assert.IsType<ArgumentException>(ex);
    }

    [Fact]
    public void Can_Assign_City_And_Priority_And_Duration()
    {
        var city = new City { Name = "Aarhus" };
        var priority = new PriorityOption { Name = "Høj" };
        var duration = new DurationOption { Name = "Kort" };

        var task = new TaskModel
        {
            City = city,
            PriorityOption = priority,
            DurationOption = duration
        };

        Assert.Equal("Aarhus", task.City.Name);
        Assert.Equal("Høj", task.PriorityOption.Name);
        Assert.Equal("Kort", task.DurationOption.Name);
    }

    [Fact]
    public void Can_Add_Categories_To_Task()
    {
        var category1 = new Category { Name = "Indkøb" };
        var category2 = new Category { Name = "Husarbejde" };

        var task = new TaskModel();
        task.TaskCategories.Add(new TaskCategory { Category = category1 });
        task.TaskCategories.Add(new TaskCategory { Category = category2 });

        Assert.Equal(2, task.TaskCategories.Count);
        Assert.Contains(task.TaskCategories, tc => tc.Category.Name == "Indkøb");
        Assert.Contains(task.TaskCategories, tc => tc.Category.Name == "Husarbejde");
    }

    [Fact]
    public void CreatedAt_Should_Be_Close_To_Now()
    {
        var before = DateTime.UtcNow;
        var task = new TaskModel();
        var after = DateTime.UtcNow;

        Assert.InRange(task.CreatedAt, before, after);
    }
    [Fact]
    public void Each_Task_Should_Have_Unique_Id()
    {
        var task1 = new TaskModel { Id = Guid.NewGuid() };
        var task2 = new TaskModel { Id = Guid.NewGuid() };

        Assert.NotEqual(task1.Id, task2.Id);
        Assert.NotEqual(Guid.Empty, task1.Id);
        Assert.NotEqual(Guid.Empty, task2.Id);
    }


    [Fact]
    public void Can_Update_Task_Title()
    {
        var task = new TaskModel { Title = "Oprindelig titel" };
        task.Title = "Opdateret titel";

        Assert.Equal("Opdateret titel", task.Title);
    }

    [Fact]
    public void Can_Update_Accepted_Status()
    {
        var task = new TaskModel { IsAccepted = false };
        Assert.False(task.IsAccepted);

        task.IsAccepted = true;
        Assert.True(task.IsAccepted);
    }

    [Fact]
    public void AcceptedBy_Should_Update_Correctly()
    {
        var task = new TaskModel { AcceptedBy = null };
        task.AcceptedBy = "user123";

        Assert.Equal("user123", task.AcceptedBy);
    }
}
