using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;
[Table("Feedback")]
public class Feedback
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = "";
    public string FeedbackText { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow; 
}