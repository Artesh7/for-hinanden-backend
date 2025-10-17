using System.ComponentModel.DataAnnotations.Schema;

namespace ForHinanden.Api.Models;

[Table("TaskCategories")]
public class TaskCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TaskId { get; set; }
    public Task Task { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}