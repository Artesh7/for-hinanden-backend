using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("TaskOffers")]
public class TaskOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }

    // Fully-qualified for at undgå forveksling med System.Threading.Tasks.Task
    public ForHinanden.Api.Models.Task Task { get; set; } = null!;

    public string OfferedBy { get; set; } = null!;   // bruger-id der anmoder
    public string? Message { get; set; }             // frivillig
    public OfferStatus Status { get; set; } = OfferStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}