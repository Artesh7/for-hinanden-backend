namespace ForHinanden.Api.Models;

public class CreateUserDto
{
    public string DeviceId  { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
    public string City      { get; set; } = null!;
    public string? ProfilePictureUrl { get; set; }

    // Valgfri
    public string? Bio { get; set; }

    // NEW: anonym tilvalg ved oprettelse (default false)
    public bool IsAnonymous { get; set; } = false;
}