﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/offers")]
public class TaskOffersController : ControllerBase
{
    private readonly AppDbContext _context;
    public TaskOffersController(AppDbContext context) => _context = context;

    // POST /api/tasks/{taskId}/offers
    [HttpPost]
    public async Task<IActionResult> CreateOffer(Guid taskId, [FromBody] CreateOfferDto dto)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task is null) return NotFound("Task findes ikke.");

        var offer = new TaskOffer
        {
            TaskId = taskId,
            OfferedBy = dto.OfferedBy,
            Message = dto.Message,
            Status = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskOffers.Add(offer);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Unikhed på (TaskId, OfferedBy) ramte – returner 409
            return Conflict("Du har allerede anmodet om at hjælpe på denne task.");
        }

        return Created($"/api/tasks/{taskId}/offers/{offer.Id}", offer);
    }

    // GET /api/tasks/{taskId}/offers
    [HttpGet]
    public async Task<IActionResult> GetOffersForTask(Guid taskId)
    {
        var exists = await _context.Tasks.AnyAsync(t => t.Id == taskId);
        if (!exists) return NotFound("Task findes ikke.");

        var items = await _context.TaskOffers
            .Where(o => o.TaskId == taskId)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        return Ok(items);
    }

    // GET /api/tasks/{taskId}/offers/by-user/{userId}
    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetOfferForUserOnTask(Guid taskId, string userId)
    {
        var offer = await _context.TaskOffers
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.TaskId == taskId && o.OfferedBy.ToLower() == userId.ToLower());

        if (offer is null) return NotFound();
        return Ok(offer);
    }

    // POST /api/tasks/{taskId}/offers/{offerId}/accept
    [HttpPost("{offerId:guid}/accept")]
    public async Task<IActionResult> AcceptOffer(Guid taskId, Guid offerId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is null) return NotFound("Task findes ikke.");

        if (task.IsAccepted) return BadRequest("Task er allerede accepteret.");

        var offer = await _context.TaskOffers.FirstOrDefaultAsync(o => o.Id == offerId && o.TaskId == taskId);
        if (offer is null) return NotFound("Offer findes ikke for denne task.");

        // Accept udvalgt offer, afvis resten
        offer.Status = OfferStatus.Accepted;
        task.IsAccepted = true;
        task.AcceptedBy = offer.OfferedBy;

        var otherOffers = await _context.TaskOffers
            .Where(o => o.TaskId == taskId && o.Id != offerId)
            .ToListAsync();

        foreach (var o in otherOffers)
            if (o.Status == OfferStatus.Pending) o.Status = OfferStatus.Rejected;

        await _context.SaveChangesAsync();

        // 👇 Navngiv felter unikt (ikke to gange "Id" i samme anonymtype)
        return Ok(new
        {
            TaskId = task.Id,
            task.AcceptedBy,
            OfferId = offer.Id,
            OfferStatus = offer.Status
        });
    }

}

// Ekstra endpoint: liste over alle offers på alle tasks for en bestemt ejer
[ApiController]
[Route("api/offers/for-owner")]
public class OffersForOwnerController : ControllerBase
{
    private readonly AppDbContext _context;
    public OffersForOwnerController(AppDbContext context) => _context = context;

    // GET /api/offers/for-owner/{ownerUserId}
    [HttpGet("{ownerUserId}")]
    public async Task<IActionResult> GetAllForOwner(string ownerUserId)
    {
        var items = await _context.TaskOffers
            .Join(
                _context.Tasks,
                o => o.TaskId,
                t => t.Id,
                (o, t) => new { o, t }
            )
            .Where(x => x.t.RequestedBy.ToLower() == ownerUserId.ToLower())
            .OrderByDescending(x => x.t.CreatedAt)
            .ThenBy(x => x.o.CreatedAt)
            .Select(x => new
            {
                TaskId = x.t.Id,
                x.t.Title,
                x.t.City,
                x.t.Priority,
                x.t.Categories,
                x.t.IsAccepted,
                x.t.AcceptedBy,
                Offer = new
                {
                    OfferId = x.o.Id,
                    x.o.OfferedBy,
                    x.o.Message,
                    x.o.Status,
                    x.o.CreatedAt
                }
            })
            .ToListAsync();

        return Ok(items);
    }
}