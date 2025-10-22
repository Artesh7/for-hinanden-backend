using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ForHinanden.Api.Models;

[Table("TaskOffers")]
public class TaskOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }

    [JsonIgnore] // 🔴 vigtig: undgå reference-cyklus ved JSON-serialisering
    public ForHinanden.Api.Models.Task Task { get; set; } = null!;

    public string OfferedBy { get; set; } = null!;
    public string? Message { get; set; }
    public OfferStatus Status { get; set; } = OfferStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}