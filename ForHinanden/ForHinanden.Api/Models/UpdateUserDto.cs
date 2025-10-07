namespace ForHinanden.Api.Models;

public class UpdateUserDto
{
    public string DeviceId  { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
    public string City      { get; set; } = null!;

    /// <summary>
    /// Send null for at lade eksisterende værdi være uændret, send "" for at sætte til null.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Samme semantik som ovenfor: null = ingen ændring, "" = sæt til null.
    /// </summary>
    public string? Bio { get; set; }
}