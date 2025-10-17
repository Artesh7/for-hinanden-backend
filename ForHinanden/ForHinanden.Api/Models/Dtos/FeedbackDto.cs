using System.Text.Json.Serialization;

namespace ForHinanden.Api.Models;

public class FeedbackDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = "";

    [JsonPropertyName("feedback")]
    public string Feedback { get; set; } = "";

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}