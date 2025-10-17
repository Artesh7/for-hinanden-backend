namespace ForHinanden.Api.Models;

public class Feedback
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = "";
    public string FeedbackText { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; 
}