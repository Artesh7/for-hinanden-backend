using System;
using System.Collections.Generic;

namespace ForHinanden.Api.Models;

public class TaskCreatedNotification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string City { get; set; } = null!;
    public TaskPriority Priority { get; set; }
    public List<string> Categories { get; set; } = new();
    public TaskDuration Duration { get; set; }
    public string RequestedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}