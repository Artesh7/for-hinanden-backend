using System;

namespace ForHinanden.Api.Models;

public class CreateMessageDto
{
    public Guid TaskId { get; set; }
    public string Sender { get; set; } = null!;
    public string Receiver { get; set; } = null!;
    public string Content { get; set; } = null!;
}