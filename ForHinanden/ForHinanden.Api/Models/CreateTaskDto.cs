namespace ForHinanden.Api.Models;

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string RequestedBy { get; set; } = null!;
}