namespace ForHinanden.Api.Models;



public class HelpRequest
{
    public Guid Id { get; set; } = Guid.NewGuid(); // automatisk ID
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string RequestedBy { get; set; } = null!;
    public string? AcceptedBy { get; set; } = null; // default null
    public bool IsAccepted { get; set; } = false;   // default false
}
