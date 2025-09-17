namespace ForHinanden.Api.Models;

public class CreateUserProfileDto
{
    public string UserName { get; set; } = null!;
    public string? FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
}