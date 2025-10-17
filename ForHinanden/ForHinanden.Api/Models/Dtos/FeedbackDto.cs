using System;
using System.Text.Json.Serialization;

namespace ForHinanden.Api.Models.Dtos;

public class FeedbackDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = "";

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("feedback")]
    public string? Feedback { get; set; }
}