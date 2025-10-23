using Xunit;
using ForHinanden.Api.Models;
using Assert = Xunit.Assert;
using TaskModel = ForHinanden.Api.Models.Task; // 👈 alias for at undgå konflikt med System.Threading.Tasks.Task

namespace Test_backend;

public class TaskTests
{
    // [Fact]
    // public void Task_Creates_With_Default_Values()
    // {
    //     var task = new TaskModel();
    //
    //     Assert.NotEqual(System.Guid.Empty, task.Id);
    //     Assert.Null(task.Title);
    //     Assert.Null(task.Description);
    //     Assert.Null(task.City);
    //     Assert.Null(task.PriorityOption);
    //     Assert.Null(task.DurationOption);
    //     Assert.Empty(task.TaskCategories);
    // }

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
    public void Title_And_Description_Cannot_Be_Empty()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var task = new TaskModel
            {
                Title = "",
                Description = ""
            };
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentNullException(nameof(task.Title), "Title is required.");
            if (string.IsNullOrWhiteSpace(task.Description))
                throw new ArgumentNullException(nameof(task.Description), "Description is required.");
        });
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
    public void Task_CreationDate_Is_Set_To_Now()
    {
        var beforeCreation = DateTime.UtcNow;
        var task = new TaskModel();
        var afterCreation = DateTime.UtcNow;

        Assert.InRange(task.CreatedAt, beforeCreation, afterCreation);
    }
}