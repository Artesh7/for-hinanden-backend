using Microsoft.AspNetCore.Mvc;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;
using TaskModel = ForHinanden.Api.Models.Task; // undgå konflikt med System.Threading.Tasks.Task

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/tasks")] // kun liste, opret, accept
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;
    public TaskController(AppDbContext context) => _context = context;

    // GET /api/tasks
    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetAll()
    {
        var items = await _context.Tasks.AsNoTracking().ToListAsync();
        return Ok(items);
    }

    // POST /api/tasks
    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var task = new TaskModel
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            RequestedBy = dto.RequestedBy,
            AcceptedBy = null,
            IsAccepted = false
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Vi har ingen GetById → returnér 201 + Location
        return Created($"/api/tasks/{task.Id}", task);
    }

    // POST /api/tasks/{id}/accept
    [HttpPost("{id:guid}/accept")]
    public async System.Threading.Tasks.Task<IActionResult> Accept(Guid id, [FromBody] AcceptTaskDto body)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.AcceptedBy))
            return BadRequest("acceptedBy er påkrævet.");

        var task = await _context.Tasks.FindAsync(id);
        if (task is null) return NotFound();

        if (task.IsAccepted) return BadRequest("Opgaven er allerede accepteret.");

        task.IsAccepted = true;
        task.AcceptedBy = body.AcceptedBy.Trim();

        await _context.SaveChangesAsync();
        return Ok(task);
    }
}