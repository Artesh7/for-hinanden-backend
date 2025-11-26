// Domain/Message.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // CHANGED

namespace ForHinanden.Api.Models;

[Table("Messages")]
public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }

    [JsonIgnore]                 // CHANGED: undgå evt. cyklus hvis en Message serialiseres med Task
    public Task Task { get; set; } = null!;  // CHANGED

    public string Sender { get; set; } = null!;
    public string Receiver { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}