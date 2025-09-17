namespace ForHinanden.Api.Models;

public class Rating
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserProfileId { get; set; } // den, der bliver rated
    public string RatedBy { get; set; } = null!; // brugernavn eller id
    public int Stars { get; set; } // fx 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public UserProfile? UserProfile { get; set; }
}