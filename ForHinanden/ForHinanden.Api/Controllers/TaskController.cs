using System;
using Microsoft.AspNetCore.Mvc;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;
using TaskModel = ForHinanden.Api.Models.Task;
// +++
using Microsoft.AspNetCore.SignalR;
using ForHinanden.Api.Hubs;
// +++

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;
    // +++
    private readonly IHubContext<NotificationsHub> _hub;
    public TaskController(AppDbContext context, IHubContext<NotificationsHub> hub)
    {
        _context = context;
        _hub = hub;
    }
    // +++

    // GET /api/tasks
    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetAll()
    {
        var items = await _context.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

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
            City = dto.City,
            Priority = dto.Priority,
            Categories = dto.Categories ?? new(),
            Duration = dto.Duration,
            AcceptedBy = null,
            IsAccepted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // +++ Send notifikation til ALLE forbundne klienter
        var notification = new TaskCreatedNotification
        {
            Id = task.Id,
            Title = task.Title,
            City = task.City,
            Priority = task.Priority,
            Categories = task.Categories,
            Duration = task.Duration,
            RequestedBy = task.RequestedBy,
            CreatedAt = task.CreatedAt
        };

        await _hub.Clients.All.SendAsync("taskCreated", notification);
        // +++

        return Created($"/api/tasks/{task.Id}", task);
    }

    // (Legacy accept)
    [HttpPost("{id:guid}/accept")]
    public async System.Threading.Tasks.Task<IActionResult> AcceptLegacy(Guid id, [FromBody] AcceptTaskDto body)
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
