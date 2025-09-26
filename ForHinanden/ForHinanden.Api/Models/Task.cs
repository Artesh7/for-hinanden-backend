using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("HelpRequests")]
public class Task
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    // Den der har oprettet opgaven (bruger-id som string)
    public string RequestedBy { get; set; } = null!;

    // Ny: by (city)
    public string City { get; set; } = null!;

    // Ny: prioritet
    public TaskPriority Priority { get; set; }

    // Ny: flere kategorier – gemmes som Postgres text[] via Npgsql (List<string>)
    public List<string> Categories { get; set; } = new();

    // Accept-information
    public string? AcceptedBy { get; set; } = null;
    public bool IsAccepted { get; set; } = false;

    // Ny: hvornår opgaven blev oprettet (server-sat)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Ny: forventet varighed
    public TaskDuration Duration { get; set; }

    // Navigation
    public ICollection<TaskOffer> Offers { get; set; } = new List<TaskOffer>();
}