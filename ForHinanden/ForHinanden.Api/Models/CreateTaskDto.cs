using System.Collections.Generic;
using ForHinanden.Api.Models;

namespace ForHinanden.Api.Models;

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string RequestedBy { get; set; } = null!; // bruger-id
    public string City { get; set; } = null!;        // "by"
    public TaskPriority Priority { get; set; }       // snart/haster/fleksibel
    public List<string> Categories { get; set; } = new(); // fx ["havearbejde","dyr"]
    
    // Forventet varighed
    public TaskDuration Duration { get; set; }
}