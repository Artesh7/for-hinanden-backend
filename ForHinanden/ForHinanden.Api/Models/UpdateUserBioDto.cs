namespace ForHinanden.Api.Models;

public class UpdateUserBioDto
{
    public string DeviceId { get; set; } = null!;
    /// <summary>
    /// Ny bio. Send "" for at slette (sætte til null).
    /// </summary>
    public string? Bio { get; set; }
}