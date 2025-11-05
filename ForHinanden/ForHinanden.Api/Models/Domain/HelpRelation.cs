using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

// Undirected relation indicating that two users have helped each other on a specific task.
// Store the pair in canonical order (UserA comes before or equals UserB) to avoid duplicates.
[Table("HelpRelations")]
public class HelpRelation
{
  [Key]
  public Guid Id { get; set; }

  [Required]
  [MaxLength(200)]
  public string UserA { get; set; } = null!; // DeviceId (lexicographically smaller or equal)

  [Required]
  [MaxLength(200)]
  public string UserB { get; set; } = null!; // DeviceId (lexicographically greater or equal)

  [Required]
  public Guid TaskId { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
