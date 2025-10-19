using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("DurationOptions")]
public class DurationOption
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}