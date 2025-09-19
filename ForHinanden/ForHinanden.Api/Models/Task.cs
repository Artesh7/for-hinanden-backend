using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("HelpRequests")]
public class Task
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string RequestedBy { get; set; } = null!;
    public string? AcceptedBy { get; set; } = null;
    public bool IsAccepted { get; set; } = false;
}