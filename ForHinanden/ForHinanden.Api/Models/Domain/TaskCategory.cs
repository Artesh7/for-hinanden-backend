// Domain/TaskCategory.cs
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // CHANGED

namespace ForHinanden.Api.Models;

[Table("TaskCategories")]
public class TaskCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }

    [JsonIgnore]                 // CHANGED: stop reference-cyklus (Task -> TaskCategories -> Task -> ...)
    public Task Task { get; set; } = null!;  // CHANGED

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}