using ForHinanden.Api.Models;
using Xunit;
using Assert = Xunit.Assert;


namespace Test;

public class PriorityOptionTests
{
    [Fact]
    public void PriorityOption_Creates_With_Required_Name()
    {
        var priorityOption = new PriorityOption { Name = "High" };
        Assert.Equal("High", priorityOption.Name);
        Assert.NotEqual(System.Guid.Empty, priorityOption.Id);
    }
    
    [Fact]
    public void PriorityOption_Name_Cannot_Be_Empty()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var priorityOption = new PriorityOption { Name = "" };
            if (string.IsNullOrWhiteSpace(priorityOption.Name))
                throw new ArgumentNullException(nameof(priorityOption.Name), "Priority option name is required.");
        });
    }
    
    [Fact]
    public void PriorityOption_Id_Is_Unique()
    {
        var option1 = new PriorityOption { Name = "Low" };
        var option2 = new PriorityOption { Name = "Medium" };
        
        Assert.NotEqual(option1.Id, option2.Id);
    }
    
    [Fact]
    public void Can_Update_PriorityOption_Name()
    {
        var priorityOption = new PriorityOption { Name = "Old Name" };
        priorityOption.Name = "New Name";
        
        Assert.Equal("New Name", priorityOption.Name);
    }
    
    [Fact]
    public void PriorityOption_Defaults_Are_Correct()
    {
        var priorityOption = new PriorityOption();
        
        Assert.NotEqual(System.Guid.Empty, priorityOption.Id);
        Assert.Null(priorityOption.Name);
    }
}