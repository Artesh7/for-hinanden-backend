namespace ForHinanden.Api.Models;

public class HelpRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string Description { get; set; }
    public string RequestedBy { get; set; } // bruger1 / bruger2
    public string? OfferedBy { get; set; }
    public bool IsAccepted { get; set; } = false;
}
