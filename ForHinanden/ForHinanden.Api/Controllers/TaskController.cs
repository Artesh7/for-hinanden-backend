using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;
using TaskModel = ForHinanden.Api.Models.Task;
using Microsoft.AspNetCore.SignalR;
using ForHinanden.Api.Hubs;
using ForHinanden.Api.Models.Dtos;

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationsHub> _hub;

    public TaskController(AppDbContext context, IHubContext<NotificationsHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    // GET /api/tasks
    [HttpGet]
    public async System.Threading.Tasks.Task<IActionResult> GetAll()
    {
        var tasks = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.City)
            .Include(t => t.PriorityOption)
            .Include(t => t.DurationOption)
            .Include(t => t.TaskCategories).ThenInclude(tc => tc.Category)
            .Include(t => t.TaskOffers)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var items = tasks.Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.RequestedBy,

            City = new { id = t.CityId, name = t.City.Name },
            Priority = new { id = t.PriorityOptionId, name = t.PriorityOption.Name },
            Duration = new { id = t.DurationOptionId, name = t.DurationOption.Name },

            Categories = t.TaskCategories
                .Select(tc => new { id = tc.CategoryId, name = tc.Category.Name })
                .ToList(),
            Offers = t.TaskOffers
                .Select(o => new
                {
                    o.Id,
                    o.OfferedBy,
                    o.Message,
                    o.Status,
                    o.CreatedAt
                })
                .ToList(),

            t.IsAccepted,
            t.AcceptedBy,
            t.CreatedAt
        });

        return Ok(items);
    }

    // GET /api/tasks/options
    [HttpGet("options")]
    public async System.Threading.Tasks.Task<IActionResult> GetOptions()
    {
        var priorities = await _context.PriorityOptions
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new { id = p.Id, name = p.Name })
            .ToListAsync();

        var durations = await _context.DurationOptions
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new { id = d.Id, name = d.Name })
            .ToListAsync();

        var cities = await _context.Cities
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { id = c.Id, name = c.Name })
            .ToListAsync();

        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { id = c.Id, name = c.Name })
            .ToListAsync();

        return Ok(new { priorities, durations, cities, categories });
    }
    
    
    // GET /api/tasks/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var task = await _context.Tasks
            .Include(t => t.TaskCategories)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return NotFound();
        return Ok(task);
    }

    // POST /api/tasks
    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        if (dto is null) return BadRequest("Body is required.");
        if (dto.CityId == Guid.Empty) return BadRequest("CityId is required.");
        if (dto.PriorityOptionId == Guid.Empty) return BadRequest("PriorityOptionId is required.");
        if (dto.DurationOptionId == Guid.Empty) return BadRequest("DurationOptionId is required.");

        // --- City (id -> entity) ---
        var city = await _context.Cities
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.CityId);
        if (city is null) return BadRequest("Invalid CityId.");

        // --- Priority option (GUID) ---
        var priorityOpt = await _context.PriorityOptions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.PriorityOptionId && p.IsActive);
        if (priorityOpt is null) return BadRequest("Invalid or inactive PriorityOptionId.");

        // --- Duration option (GUID) ---
        var durationOpt = await _context.DurationOptions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dto.DurationOptionId && d.IsActive);
        if (durationOpt is null) return BadRequest("Invalid or inactive DurationOptionId.");

        // --- Categories (ids -> entities) ---
        var categoryIds = (dto.CategoryIds ?? new())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var existingCats = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync();

        var missing = categoryIds.Except(existingCats.Select(c => c.Id)).ToList();
        if (missing.Count > 0)
            return BadRequest($"Unknown CategoryIds: {string.Join(", ", missing)}");

        // --- Build Task entity (store FK GUIDs) ---
        var task = new TaskModel
        {
            Id = Guid.NewGuid(),
            Title = dto.Title?.Trim() ?? "",
            Description = dto.Description?.Trim() ?? "",
            RequestedBy = dto.RequestedBy?.Trim() ?? "",

            CityId = city.Id,
            PriorityOptionId = priorityOpt.Id,
            DurationOptionId = durationOpt.Id,

            IsAccepted = false,
            AcceptedBy = null,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var catId in existingCats.Select(c => c.Id))
        {
            task.TaskCategories.Add(new TaskCategory
            {
                Task = task,
                CategoryId = catId
            });
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // --- Real-time notification (optional) ---
        var notification = new
        {
            Id = task.Id,
            Title = task.Title,
            City = new { id = city.Id, name = city.Name },
            Priority = new { id = priorityOpt.Id, name = priorityOpt.Name },
            Duration = new { id = durationOpt.Id, name = durationOpt.Name },
            Categories = existingCats.Select(c => new { id = c.Id, name = c.Name }).ToList(),
            RequestedBy = task.RequestedBy,
            CreatedAt = task.CreatedAt
        };
        await _hub.Clients.All.SendAsync("taskCreated", notification);

        // --- Return created task in list-item shape ---
        return Created($"/api/tasks/{task.Id}", new
        {
            task.Id,
            task.Title,
            task.Description,
            task.RequestedBy,

            City = new { id = city.Id, name = city.Name },
            Priority = new { id = priorityOpt.Id, name = priorityOpt.Name },
            Duration = new { id = durationOpt.Id, name = durationOpt.Name },

            Categories = existingCats.Select(c => new { id = c.Id, name = c.Name }).ToList(),

            task.IsAccepted,
            task.AcceptedBy,
            task.CreatedAt
        });
    }

    // (Legacy accept endpoint kept as-is)
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
    // PUT /api/tasks/{id}
 [HttpPut("{id:guid}")]
public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
{
    if (dto is null || id != dto.Id)
        return BadRequest("Invalid request body or mismatched task id.");

    if (dto.CityId == Guid.Empty) return BadRequest("CityId is required.");
    if (dto.PriorityOptionId == Guid.Empty) return BadRequest("PriorityOptionId is required.");
    if (dto.DurationOptionId == Guid.Empty) return BadRequest("DurationOptionId is required.");

    var city = await _context.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == dto.CityId);
    if (city is null) return BadRequest("Invalid CityId.");

    var priorityOpt = await _context.PriorityOptions.AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == dto.PriorityOptionId && p.IsActive);
    if (priorityOpt is null) return BadRequest("Invalid or inactive PriorityOptionId.");

    var durationOpt = await _context.DurationOptions.AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == dto.DurationOptionId && d.IsActive);
    if (durationOpt is null) return BadRequest("Invalid or inactive DurationOptionId.");

    // KUN valider kategorier hvis de er medsendt
    var existingCats = new List<Category>();
    if (dto.CategoryIds is { Count: > 0 })
    {
        var categoryIds = dto.CategoryIds.Where(g => g != Guid.Empty).Distinct().ToList();
        existingCats = await _context.Categories.Where(c => categoryIds.Contains(c.Id)).ToListAsync();

        var missing = categoryIds.Except(existingCats.Select(c => c.Id)).ToList();
        if (missing.Count > 0)
            return BadRequest($"Unknown CategoryIds: {string.Join(", ", missing)}");
    }

    var task = await _context.Tasks
        .Include(t => t.TaskCategories)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (task is null)
        return NotFound($"No task found with id {id}.");

    // --- Opdater kun når der er sendt en værdi (null = ingen ændring) ---
    if (dto.Title != null)        task.Title        = dto.Title.Trim();
    if (dto.Description != null)  task.Description  = dto.Description.Trim(); // aldrig null til DB
    if (dto.RequestedBy != null)  task.RequestedBy  = dto.RequestedBy.Trim();

    task.CityId           = dto.CityId;
    task.PriorityOptionId = dto.PriorityOptionId;
    task.DurationOptionId = dto.DurationOptionId;

    // Erstat kun kategorier hvis client sendte en liste
    if (dto.CategoryIds != null)
    {
        task.TaskCategories ??= new List<TaskCategory>();
        task.TaskCategories.Clear();
        foreach (var catId in existingCats.Select(c => c.Id))
        {
            task.TaskCategories.Add(new TaskCategory { Task = task, CategoryId = catId });
        }
    }

    try
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateException ex)
    {
        // valgfrit: log ex.InnerException?.Message
        return StatusCode(500, "Failed to update task due to a database error.");
    }

    return Ok(new
    {
        task.Id,
        task.Title,
        task.Description,
        task.RequestedBy,
        task.CityId,
        task.PriorityOptionId,
        task.DurationOptionId,
        task.IsAccepted,
        task.AcceptedBy
    });
}


    // DELETE /api/tasks/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task is null) return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
