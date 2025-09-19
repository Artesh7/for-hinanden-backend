namespace ForHinanden.Api.Models;

public class CreateRatingDto
{
    public Guid UserProfileId { get; set; }
    public string RatedBy { get; set; } = null!;
    public int Stars { get; set; }
    public string? Comment { get; set; }
}