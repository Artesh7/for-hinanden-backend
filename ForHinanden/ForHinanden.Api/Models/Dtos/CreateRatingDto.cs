using System;

namespace ForHinanden.Api.Models;

public class CreateRatingDto
{
    public Guid TaskId { get; set; }
    public string ToUserId { get; set; } = null!;
    public string RatedBy { get; set; } = null!;
    public int Stars { get; set; }
    public string? Comment { get; set; }
}