using System;
using System.Collections.Generic;

namespace ForHinanden.Api.Models;

public sealed class CreateTaskDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string RequestedBy { get; set; } = "";

    public Guid CityId { get; set; }                  // 👈 city as GUID
    public Guid PriorityOptionId { get; set; }        // GUID (active option)
    public Guid DurationOptionId { get; set; }        // GUID (active option)

    public List<Guid> CategoryIds { get; set; } = new();  // 👈 categories as GUIDs
}