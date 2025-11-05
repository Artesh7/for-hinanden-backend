using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("Users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string DeviceId { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    public string City { get; set; } = null!;

    /// <summary>
    /// Valgfri URL til profilbillede (fx /uploads/users/xxx.jpg eller ekstern URL).
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Valgfri bio/om mig-tekst. Max 500 tegn.
    /// </summary>
    [MaxLength(500)]
    public string? Bio { get; set; }

    // NEW: anonym brugerflag (kan toggles af brugeren)
    public bool IsAnonymous { get; set; } = false;
}