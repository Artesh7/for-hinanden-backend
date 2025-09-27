using System;
using System.Collections.Generic;

namespace ForHinanden.Api.Models;

public class TaskListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string RequestedBy { get; set; } = null!;

    public string City { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string Duration { get; set; } = null!;
    public List<string> Categories { get; set; } = new();

    public bool IsAccepted { get; set; }
    public string? AcceptedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}