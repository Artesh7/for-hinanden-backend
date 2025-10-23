using ForHinanden.Api.Models;
using Xunit;
using Xunit;
using Assert = Xunit.Assert;

namespace Test;

public class CategoryTests
{
    [Fact]
    public void Category_Creates_With_Required_Name()
    {
        var category = new Category { Name = "Transport" };
        Assert.Equal("Transport", category.Name);
        Assert.NotEqual(System.Guid.Empty, category.Id);
    }
    
    [Fact]
    public void Category_Name_Cannot_Be_Empty()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var category = new Category { Name = "" };
            if (string.IsNullOrWhiteSpace(category.Name))
                throw new ArgumentNullException(nameof(category.Name), "Category name is required.");
        });
    }
    [Fact]
    public void Category_Id_Is_Unique()
    {
        var category1 = new Category { Name = "Gardening" };
        var category2 = new Category { Name = "Cleaning" };
        
        Assert.NotEqual(category1.Id, category2.Id);
    }
    
    [Fact]
    public void Can_Update_Category_Name()
    {
        var category = new Category { Name = "Old Name" };
        category.Name = "New Name";
        
        Assert.Equal("New Name", category.Name);
    }
    
    [Fact]
    public void Category_Defaults_Are_Correct()
    {
        var category = new Category();
        
        Assert.NotEqual(System.Guid.Empty, category.Id);
        Assert.Null(category.Name);
    }
    
}