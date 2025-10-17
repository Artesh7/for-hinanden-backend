using System.Text.Json.Serialization;

namespace ForHinanden.Api.Models.Dtos;

public class UpdateFeedbackDto
{
    [JsonPropertyName("rating")]
    public int? Rating { get; set; }       // optional; if present must be 1..5

    [JsonPropertyName("feedback")]
    public string? Feedback { get; set; }  // optional; can be "" to clear, or text
}