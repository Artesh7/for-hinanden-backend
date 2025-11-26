using System;
using System.Collections.Generic;

namespace ForHinanden.Api.Models;

public class Task
{
    public Guid Id { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string RequestedBy { get; set; } = "";

    public Guid CityId { get; set; }
    public City City { get; set; } = default!;

    public Guid PriorityOptionId { get; set; }
    public PriorityOption PriorityOption { get; set; } = default!;

    public Guid DurationOptionId { get; set; }
    public DurationOption DurationOption { get; set; } = default!;

    public bool IsAccepted { get; set; }
    public string? AcceptedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskCategory> TaskCategories { get; set; } = new List<TaskCategory>();
    public ICollection<TaskOffer> TaskOffers { get; set; } = new List<TaskOffer>();

}