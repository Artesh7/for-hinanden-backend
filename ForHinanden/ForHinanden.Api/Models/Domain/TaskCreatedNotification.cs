using System;
using System.Collections.Generic;

namespace ForHinanden.Api.Models;

public class TaskCreatedNotification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public List<string> Categories { get; set; } = new();
    public string Duration { get; set; } = null!;
    public string RequestedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}