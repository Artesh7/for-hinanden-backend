using System;
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
            return Conflict("Du har allerede anmodet om at hjælpe på denne task.");
        }
        var user = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == task.RequestedBy);
        if (user != null && !string.IsNullOrWhiteSpace(user.DeviceId))
        {
            var fcmMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = user.DeviceId, // DeviceId now used as FCM token
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = "Din opgave er accepteret!",
                    Body = $"Bruger {user.FirstName} {user.LastName} har accepteret din opgave '{task.Title}'."
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
        // Find the task
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is null) return NotFound("Task findes ikke.");

        if (task.IsAccepted) return BadRequest("Task er allerede accepteret.");

        // Find the offer
        var offer = await _context.TaskOffers.FirstOrDefaultAsync(o => o.Id == offerId && o.TaskId == taskId);
        if (offer is null) return NotFound("Offer findes ikke for denne task.");

        // Accept selected offer
        offer.Status = OfferStatus.Accepted;
        task.IsAccepted = true;
        task.AcceptedBy = offer.OfferedBy;

        // Reject other pending offers
        var otherOffers = await _context.TaskOffers
            .Where(o => o.TaskId == taskId && o.Id != offerId)
            .ToListAsync();

        foreach (var o in otherOffers)
            if (o.Status == OfferStatus.Pending) o.Status = OfferStatus.Rejected;

        await _context.SaveChangesAsync();

        // --- Send Firebase notification to task creator ---
        var user = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == task.RequestedBy);
        if (user != null && !string.IsNullOrWhiteSpace(user.DeviceId))
        {
            var fcmMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = user.DeviceId, // DeviceId now used as FCM token
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = "Din opgave er accepteret!",
                    Body = $"Bruger {offer.OfferedBy} har accepteret din opgave '{task.Title}'."
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
        ownerUserId = (ownerUserId ?? string.Empty).Trim();

        var items = await _context.TaskOffers
            // Join offers ↔ tasks (with city + categories)
            .Join(
                _context.Tasks
                    .Include(t => t.City)
                    .Include(t => t.TaskCategories)
                    .ThenInclude(tc => tc.Category),
                o => o.TaskId,
                t => t.Id,
                (o, t) => new { o, t }
            )
            // Join tasks ↔ priority options (GUID FK)
            .Join(
                _context.PriorityOptions.AsNoTracking(),
                x => x.t.PriorityOptionId,
                p => p.Id,
                (x, p) => new { x.o, x.t, priority = p }
            )
            .Where(x => x.t.RequestedBy.ToLower() == ownerUserId.ToLower())
            .OrderByDescending(x => x.t.CreatedAt)
            .ThenBy(x => x.o.CreatedAt)
            .Select(x => new
            {
                TaskId = x.t.Id,
                x.t.Title,
                City = new { id = x.t.CityId, name = x.t.City.Name },
                Priority = new { id = x.priority.Id, name = x.priority.Name },
                Categories = x.t.TaskCategories
                    .Select(tc => new { id = tc.CategoryId, name = tc.Category.Name })
                    .ToList(),
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