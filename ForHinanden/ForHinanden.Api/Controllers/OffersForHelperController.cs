using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ForHinanden.Api.Data;

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/offers/for-helper")]
public class OffersForHelperController : ControllerBase
{
    private readonly AppDbContext _context;

    public OffersForHelperController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/offers/for-helper/{helperId}
    [HttpGet("{helperId}")]
    public async Task<IActionResult> GetAllForHelper(string helperId)
    {
        if (string.IsNullOrWhiteSpace(helperId))
            return BadRequest("HelperId er påkrævet.");

        helperId = helperId.Trim().ToLower();

        var items = await _context.TaskOffers
            .Join(
                _context.Tasks
                    .Include(t => t.City)
                    .Include(t => t.PriorityOption)
                    .Include(t => t.DurationOption)
                    .Include(t => t.TaskCategories)
                        .ThenInclude(tc => tc.Category),
                offer => offer.TaskId,
                task => task.Id,
                (offer, task) => new { offer, task }
            )
            .Join(
                _context.Users.AsNoTracking(),
                x => x.task.RequestedBy,
                u => u.DeviceId,
                (x, u) => new { x.offer, x.task, requester = u }
            )
            .Where(x => x.offer.OfferedBy.ToLower() == helperId)
            .OrderByDescending(x => x.offer.CreatedAt)
            .Select(x => new
            {
                TaskId = x.task.Id,
                TaskTitle = x.task.Title,
                RequestedByName = $"{x.requester.FirstName} {x.requester.LastName}".Trim(),
                City = x.task.City != null ? x.task.City.Name : null,
                Priority = x.task.PriorityOption.Name,
                Duration = x.task.DurationOption.Name,
                Categories = x.task.TaskCategories
                    .Select(tc => new { id = tc.CategoryId, name = tc.Category.Name })
                    .ToList(),
                OfferMessage = x.offer.Message,
                OfferStatus = x.offer.Status.ToString(),
                CreatedAt = x.offer.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }
}
