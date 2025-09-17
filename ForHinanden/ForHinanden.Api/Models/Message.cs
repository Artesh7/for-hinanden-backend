using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("Messages")]
public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!; // kobling til en specifik opgave
    public string Sender { get; set; } = null!;  // fx brugerens navn eller Id
    public string Receiver { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
