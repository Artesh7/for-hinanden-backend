using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;                 // CHANGED
using ForHinanden.Api.Data;
using ForHinanden.Api.Hubs;
using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/offers")]
public class TaskOffersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationsHub> _hub;

    public TaskOffersController(AppDbContext context, IHubContext<NotificationsHub> hub)
    {
        _context = context;
        _hub = hub;
    }
    
    // POST /api/tasks/{taskId}/offers
    [HttpPost]
    public async Task<IActionResult> CreateOffer(Guid taskId, [FromBody] CreateOfferDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.OfferedBy))
            return BadRequest("OfferedBy er påkrævet.");

        var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is null) return NotFound("Task findes ikke.");

        // Må ikke byde på egen opgave
        if (task.RequestedBy.Equals(dto.OfferedBy, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Du kan ikke tilbyde hjælp til din egen opgave.");

        // Opgaven er allerede accepteret?
        if (task.IsAccepted)
            return Conflict("Opgaven er allerede accepteret.");

        // Dublet-beskyttelse (udover DB-constraint)
        var exists = await _context.TaskOffers
            .AnyAsync(o => o.TaskId == taskId &&
                           o.OfferedBy.ToLower() == dto.OfferedBy.ToLower());
        if (exists)
            return Conflict("Du har allerede anmodet om at hjælpe på denne task.");

        var offer = new TaskOffer
        {
            TaskId = taskId,
            OfferedBy = dto.OfferedBy.Trim(),
            Message = string.IsNullOrWhiteSpace(dto.Message) ? null : dto.Message!.Trim(),
            Status  = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.TaskOffers.Add(offer);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // fx unique constraint (TaskId, OfferedBy)
            return Conflict("Du har allerede anmodet om at hjælpe på denne task.");
        }

        // --- FCM: send til opgavens ejer (RequestedBy), ikke hjælperen ---
        try
        {
            var owner  = await _context.Users.AsNoTracking()
                             .FirstOrDefaultAsync(u => u.DeviceId == task.RequestedBy);
            var helper = await _context.Users.AsNoTracking()
                             .FirstOrDefaultAsync(u => u.DeviceId == offer.OfferedBy);

            if (owner != null && !string.IsNullOrWhiteSpace(owner.DeviceId))
            {
                var helperName = helper != null
                    ? $"{helper.FirstName} {helper.LastName}".Trim()
                    : offer.OfferedBy;

                var fcmMessage = new FirebaseAdmin.Messaging.Message
                {
                    Token = owner.DeviceId, // ✅ send til ejerens FCM-token
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = $"{helperName} har tilbudt hjælp til din opgave",
                        Body  = string.IsNullOrWhiteSpace(offer.Message)
                                ? $"Opgave: {task.Title}"
                                : offer.Message
                    },
                    Data = new Dictionary<string, string>           // CHANGED: data payload til deep link
                    {
                        ["type"]         = "offer_received",        // CHANGED
                        ["taskId"]       = task.Id.ToString(),      // CHANGED
                        ["route"]        = "task_offers",           // CHANGED (GoRouter-rutenavn)
                        ["click_action"] = "FLUTTER_NOTIFICATION_CLICK" // CHANGED (Android)
                    }
                };

                await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
            }
        }
        catch (Exception ex)
        {
            // Log, men lad ikke API-kaldet fejle
            Console.WriteLine($"FCM notification failed: {ex.Message}");
        }

        // Returnér et DTO uden navigationer → ingen cyklus-risiko
        var dtoOut = new ForHinanden.Api.Models.Dtos.TaskOfferDto(
            offer.Id, offer.TaskId, offer.OfferedBy, offer.Message, offer.Status, offer.CreatedAt
        );

        return CreatedAtAction(nameof(GetOfferForUserOnTask),
            new { taskId, userId = offer.OfferedBy }, dtoOut);
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
            .Select(o => new ForHinanden.Api.Models.Dtos.TaskOfferDto(
                o.Id, o.TaskId, o.OfferedBy, o.Message, o.Status, o.CreatedAt))
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

        var dto = new ForHinanden.Api.Models.Dtos.TaskOfferDto(
            offer.Id, offer.TaskId, offer.OfferedBy, offer.Message, offer.Status, offer.CreatedAt);

        return Ok(dto);
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

        // Accept valgt tilbud
        offer.Status = OfferStatus.Accepted;
        task.IsAccepted = true;
        task.AcceptedBy = offer.OfferedBy;

        // Afvis øvrige
        var otherOffers = await _context.TaskOffers
            .Where(o => o.TaskId == taskId && o.Id != offerId)
            .ToListAsync();

        foreach (var o in otherOffers)
            if (o.Status == OfferStatus.Pending) o.Status = OfferStatus.Rejected;

        await _context.SaveChangesAsync();

        // NEW: Persist help relation between requester and accepted helper (undirected)
        try
        {
            var req = task.RequestedBy?.Trim() ?? string.Empty;
            var hel = task.AcceptedBy?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(req) && !string.IsNullOrWhiteSpace(hel))
            {
                var a = string.CompareOrdinal(req, hel) <= 0 ? req : hel;
                var b = string.CompareOrdinal(req, hel) <= 0 ? hel : req;

                var exists = await _context.HelpRelations
                    .AsNoTracking()
                    .AnyAsync(hr => hr.TaskId == task.Id && hr.UserA == a && hr.UserB == b);
                if (!exists)
                {
                    _context.HelpRelations.Add(new HelpRelation
                    {
                        Id = Guid.NewGuid(),
                        TaskId = task.Id,
                        UserA = a,
                        UserB = b,
                        CreatedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
            }
        }
        catch { /* ignore relation persistence errors */ }

        // --- FCM: send resultat til hjælperen (den accepterede) ---
        var helper = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == offer.OfferedBy);
        var owner  = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == task.RequestedBy);
        if (helper != null && !string.IsNullOrWhiteSpace(helper.DeviceId))
        {
            var fcmMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = helper.DeviceId, // DeviceId bruges som FCM token i dit setup
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = "Din anmodning er accepteret!",
                    Body  = owner != null
                        ? $"{owner.FirstName} {owner.LastName} har accepteret din hjælp til '{task.Title}'."
                        : $"Din hjælp til '{task.Title}' er accepteret."
                },
                Data = new Dictionary<string, string>              // CHANGED: også deeplink ved accept
                {
                    ["type"]         = "offer_accept_result",      // CHANGED
                    ["taskId"]       = task.Id.ToString(),         // CHANGED
                    ["route"]        = "task_offers",              // CHANGED (valgfrit)
                    ["click_action"] = "FLUTTER_NOTIFICATION_CLICK"// CHANGED
                }
            };

            try
            {
                await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FCM notification failed: {ex.Message}");
            }
        }

        return Ok(new
        {
            TaskId = task.Id,
            task.AcceptedBy,
            OfferId = offer.Id,
            OfferStatus = offer.Status
        });
    }

    // DELETE /api/tasks/{taskId}/offers/{offeredBy}
    [HttpDelete("{offeredBy}")]
    public async Task<IActionResult> CancelOffer(Guid taskId, string offeredBy)
    {
        var offer = await _context.TaskOffers
            .FirstOrDefaultAsync(o => o.TaskId == taskId && o.OfferedBy == offeredBy);

        if (offer is null)
            return NotFound($"Ingen tilbud fundet for bruger '{offeredBy}' på opgaven {taskId}.");

        _context.TaskOffers.Remove(offer);
        await _context.SaveChangesAsync();

        await _hub.Clients.All.SendAsync("offerCancelled", new { taskId, offeredBy });
        return NoContent();
    }

    // PUT /api/tasks/{id}/complete
    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> MarkTaskCompleted(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        await _hub.Clients.All.SendAsync("feedbackRequested", new
        {
            taskId = task.Id,
            title = task.Title,
            requestedBy = task.RequestedBy
        });

        return Ok(new { message = "Feedback notification sent." });
    }
}
