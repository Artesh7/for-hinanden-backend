using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForHinanden.Api.Models;

[Table("Feedback")]
[Index(nameof(DeviceId), IsUnique = true)] // én feedback pr. device
public class Feedback
{
    public int Id { get; set; }

    [Required]
    public string DeviceId { get; set; } = "";

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? FeedbackText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}