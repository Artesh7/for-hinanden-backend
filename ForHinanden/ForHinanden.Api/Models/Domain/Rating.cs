using System;
using System.ComponentModel.DataAnnotations;

namespace ForHinanden.Api.Models;

public class Rating
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Task som ratingen vedrører
    public Guid TaskId { get; set; }

    // Hvem MODTAGER ratingen (bruger-id)
    public string ToUserId { get; set; } = null!;

    // Hvem GIVER ratingen (bruger-id)
    public string RatedBy { get; set; } = null!;

    [Range(1, 5)]
    public int Stars { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}