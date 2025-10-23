using ForHinanden.Api.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace Test;

public class UserTests
{
    [Fact]
    public void User_Creates_With_Required_Fields()
    {
        var user = new User
        {
            DeviceId = "device123",
            FirstName = "Ahmed",
            LastName = "Ali",
            City = "Aarhus"
        };

        Assert.Equal("device123", user.DeviceId);
        Assert.Equal("Ahmed", user.FirstName);
        Assert.Equal("Ali", user.LastName);
        Assert.Equal("Aarhus", user.City);
    }

    [Fact]
    public void User_Can_Have_Optional_ProfilePicture_And_Bio()
    {
        var user = new User
        {
            DeviceId = "device456",
            FirstName = "Luna",
            LastName = "Jensen",
            City = "Odense",
            ProfilePictureUrl = "/uploads/users/luna.jpg",
            Bio = "Jeg elsker at hjælpe andre!"
        };

        Assert.Equal("/uploads/users/luna.jpg", user.ProfilePictureUrl);
        Assert.Equal("Jeg elsker at hjælpe andre!", user.Bio);
    }

    [Fact]
    public void User_Bio_Can_Be_Null()
    {
        var user = new User
        {
            DeviceId = "device789",
            FirstName = "Sara",
            LastName = "Møller",
            City = "København"
        };

        Assert.Null(user.Bio);
    }
    
    [Fact]
    public void User_Id_Is_Unique()
    {
        var user1 = new User { DeviceId = "device001", FirstName = "Nina", LastName = "Hansen", City = "Aalborg" };
        var user2 = new User { DeviceId = "device002", FirstName = "Mikkel", LastName = "Larsen", City = "Esbjerg" };
        
        Assert.NotEqual(user1.DeviceId, user2.DeviceId);
    }
    
    [Fact]
    public void User_Defaults_Are_Correct()
    {
        var user = new User();
        
        Assert.NotEqual(System.Guid.Empty.ToString(), user.DeviceId);
        Assert.Null(user.DeviceId);
        Assert.Null(user.FirstName);
        Assert.Null(user.LastName);
        Assert.Null(user.City);
        Assert.Null(user.ProfilePictureUrl);
        Assert.Null(user.Bio);
    }
    
    [Fact]
    public void Can_Update_User_City()
    {
        var user = new User { DeviceId = "device003", FirstName = "Emil", LastName = "Nielsen", City = "Roskilde" };
        user.City = "Helsingør";
        
        Assert.Equal("Helsingør", user.City);
    }
    
    [Fact]
    public void Can_Update_User_Bio()
    {
        var user = new User { DeviceId = "device004", FirstName = "Katrine", LastName = "Olsen", City = "Vejle" };
        user.Bio = "Ny bio tekst";
        
        Assert.Equal("Ny bio tekst", user.Bio);
    }
    
    [Fact]
    public void User_FirstName_Cannot_Be_Empty()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var user = new User { DeviceId = "device005", FirstName = "", LastName = "Poulsen", City = "Horsens" };
            if (string.IsNullOrWhiteSpace(user.FirstName))
                throw new ArgumentNullException(nameof(user.FirstName), "First name is required.");
        });
    }
}