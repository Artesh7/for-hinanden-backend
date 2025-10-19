using System.Text.Json.Serialization;

namespace ForHinanden.Api.Models.Dtos;

public class FeedbackDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = "";

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("emojiLabel")]
    public string? EmojiLabel { get; set; }

    [JsonPropertyName("improvementText")]
    public string? ImprovementText { get; set; }

    [JsonPropertyName("volunteerOpinion")]
    public string? VolunteerOpinion { get; set; }

    [JsonPropertyName("feedbackText")]
    public string? FeedbackText { get; set; }

    [JsonPropertyName("feedback")]
    public string? Feedback
    {
        get => FeedbackText;
        set => FeedbackText = value;
    }
}