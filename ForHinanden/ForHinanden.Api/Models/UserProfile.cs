using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("UserProfiles")]
public class UserProfile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserName { get; set; } = null!;

    public string? FullName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }

    // Statistik – fx tidligere hjælp
    public int TasksCreated { get; set; } = 0;
    public int TasksCompleted { get; set; } = 0;
    public double? Rating { get; set; } // gennemsnitlig vurdering
}