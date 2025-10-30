﻿using System;
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
            Categories = t.TaskCategories.Select(tc => new { id = tc.CategoryId, name = tc.Category.Name }).ToList(),
            Offers = t.TaskOffers.Select(o => new { o.Id, o.OfferedBy, o.Message, o.Status, o.CreatedAt }).ToList(),
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
        var dto = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Include(t => t.City)
            .Include(t => t.PriorityOption)
            .Include(t => t.DurationOption)
            .Include(t => t.TaskCategories).ThenInclude(tc => tc.Category)
            .Select(t => new TaskListItemDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                RequestedBy = t.RequestedBy,
                City = t.City.Name,
                Priority = t.PriorityOption.Name,
                Duration = t.DurationOption.Name,
                Categories = t.TaskCategories.Select(tc => tc.Category.Name).ToList(),
                IsAccepted = t.IsAccepted,
                AcceptedBy = t.AcceptedBy,
                CreatedAt = t.CreatedAt
            })
            .SingleOrDefaultAsync();

        return dto is null ? NotFound() : Ok(dto);
    }


    // POST /api/tasks
    [HttpPost]
    public async System.Threading.Tasks.Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        if (dto is null) return BadRequest("Body is required.");
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
            task.TaskCategories.Add(new TaskCategory { Task = task, CategoryId = catId });

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.DeviceId == task.RequestedBy);

        if (user != null && !string.IsNullOrWhiteSpace(user.DeviceId))
        {
            var fcmMessage = new FirebaseAdmin.Messaging.Message
            {
                Topic = "allUsers",
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = "En person i nærheden har brug for hjælp!",
                    Body = $"{task.Title}"
                },
                Data = new Dictionary<string, string>
                {
                    { "type", "task" },
                    { "taskId", task.Id.ToString() },
                    { "title", task.Title ?? "" },
                    { "route", "/feed?highlight=${task.Id}" }
                }
            };

            try
            {
                await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the API call
                Console.WriteLine($"FCM notification failed: {ex.Message}");
            }
        }

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

    // (Legacy accept endpoint)
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

    // PUT /api/tasks/{id} — server-side update + server-side kategori-delta
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto dto)
    {
        if (dto is null || id != dto.Id)
            return BadRequest("Invalid request body or mismatched task id.");

        if (dto.CityId == Guid.Empty) return BadRequest("CityId is required.");
        if (dto.PriorityOptionId == Guid.Empty) return BadRequest("PriorityOptionId is required.");
        if (dto.DurationOptionId == Guid.Empty) return BadRequest("DurationOptionId is required.");

        // Validate lookups
        var cityExists = await _context.Cities.AsNoTracking().AnyAsync(c => c.Id == dto.CityId);
        if (!cityExists) return BadRequest("Invalid CityId.");

        var prioExists = await _context.PriorityOptions.AsNoTracking()
            .AnyAsync(p => p.Id == dto.PriorityOptionId && p.IsActive);
        if (!prioExists) return BadRequest("Invalid or inactive PriorityOptionId.");

        var durExists = await _context.DurationOptions.AsNoTracking()
            .AnyAsync(d => d.Id == dto.DurationOptionId && d.IsActive);
        if (!durExists) return BadRequest("Invalid or inactive DurationOptionId.");

        // --- 1) Server-side UPDATE (undgår tracking/concurrency) ---
        var rows = await _context.Tasks
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Title,        t => dto.Title != null       ? dto.Title.Trim()       : t.Title)
                .SetProperty(t => t.Description,  t => dto.Description != null ? dto.Description.Trim() : t.Description)
                .SetProperty(t => t.RequestedBy,  t => dto.RequestedBy != null ? dto.RequestedBy.Trim() : t.RequestedBy)
                .SetProperty(t => t.CityId,           t => dto.CityId)
                .SetProperty(t => t.PriorityOptionId, t => dto.PriorityOptionId)
                .SetProperty(t => t.DurationOptionId, t => dto.DurationOptionId)
            );

        if (rows == 0) return NotFound($"No task found with id {id}.");

        // --- 2) Server-side kategori-delta (kun hvis klienten sendte CategoryIds) ---
        if (dto.CategoryIds != null)
        {
            var wanted = dto.CategoryIds
                .Where(g => g != Guid.Empty)
                .Distinct()
                .ToList();

            if (wanted.Count > 0)
            {
                var existing = await _context.Categories
                    .Where(c => wanted.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                var missing = wanted.Except(existing).ToList();
                if (missing.Count > 0)
                    return BadRequest($"Unknown CategoryIds: {string.Join(", ", missing)}");
            }

            // Slet alle relationer der ikke længere ønskes (server-side)
            await _context.TaskCategories
                .Where(tc => tc.TaskId == id && !wanted.Contains(tc.CategoryId))
                .ExecuteDeleteAsync();

            // Hent nuværende efter sletning
            var current = await _context.TaskCategories
                .Where(tc => tc.TaskId == id)
                .Select(tc => tc.CategoryId)
                .ToListAsync();

            var toAdd = wanted.Except(current).ToList();
            if (toAdd.Count > 0)
            {
                _context.TaskCategories.AddRange(
                    toAdd.Select(catId => new TaskCategory { TaskId = id, CategoryId = catId })
                );
                await _context.SaveChangesAsync();
            }
        }

        // Returnér opdateret “flat” task
        var result = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.RequestedBy,
                t.CityId,
                t.PriorityOptionId,
                t.DurationOptionId,
                t.IsAccepted,
                t.AcceptedBy
            })
            .FirstAsync();

        return Ok(result);
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
