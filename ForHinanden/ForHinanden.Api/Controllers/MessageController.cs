using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/messages")]
public class MessageController : ControllerBase
{
    private readonly AppDbContext _context;
    public MessageController(AppDbContext context) => _context = context;

    // GET: api/messages/{taskId}
    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetByTask(Guid taskId)
    {
        var messages = await _context.Messages
            .Where(m => m.TaskId == taskId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
        return Ok(messages);
    }

    // POST: api/messages (kun hvis task er accepteret, og kun mellem de to parter)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessageDto dto)
    {
        if (dto is null) return BadRequest("Body is required.");
        if (dto.TaskId == Guid.Empty) return BadRequest("taskId is required.");
        if (string.IsNullOrWhiteSpace(dto.Sender)) return BadRequest("sender is required.");
        if (string.IsNullOrWhiteSpace(dto.Receiver)) return BadRequest("receiver is required.");
        if (string.IsNullOrWhiteSpace(dto.Content)) return BadRequest("content is required.");

        var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.TaskId);
        if (task is null) return NotFound("Task findes ikke.");

        if (!task.IsAccepted || string.IsNullOrWhiteSpace(task.AcceptedBy))
            return StatusCode(403, "Du kan ikke sende beskeder før en anmodning er accepteret."); // 403 i stedet for Forbid("...")

        var s = dto.Sender.Trim();
        var r = dto.Receiver.Trim();
        var req = (task.RequestedBy ?? string.Empty).Trim();
        var acc = (task.AcceptedBy ?? string.Empty).Trim();

        bool pairOk =
            (s.Equals(req, StringComparison.OrdinalIgnoreCase) && r.Equals(acc, StringComparison.OrdinalIgnoreCase)) ||
            (s.Equals(acc, StringComparison.OrdinalIgnoreCase) && r.Equals(req, StringComparison.OrdinalIgnoreCase));

        if (!pairOk)
            return StatusCode(403, "Beskeder må kun sendes mellem task-ejer og den accepterede hjælper."); // 403 m. tekst

        var message = new Message
        {
            TaskId = dto.TaskId,
            Sender = s,
            Receiver = r,
            Content = dto.Content
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Created($"/api/messages/{message.Id}", message);
    }

    // GET: api/messages/conversation?user1=...&user2=...
    [HttpGet("conversation")]
    public async Task<IActionResult> GetConversation([FromQuery] string user1, [FromQuery] string user2)
    {
        if (string.IsNullOrWhiteSpace(user1) || string.IsNullOrWhiteSpace(user2))
            return BadRequest("Both user1 and user2 must be provided.");

        var messages = await _context.Messages
            .Where(m =>
                (m.Sender.ToLower() == user1.ToLower() && m.Receiver.ToLower() == user2.ToLower()) ||
                (m.Sender.ToLower() == user2.ToLower() && m.Receiver.ToLower() == user1.ToLower()))
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

    // ✅ Hjælp: find seneste accepterede task mellem to brugere (til test i Swagger)
    // GET: api/messages/eligible-task?user1=...&user2=...
    [HttpGet("eligible-task")]
    public async Task<IActionResult> GetEligibleTask([FromQuery] string user1, [FromQuery] string user2)
    {
        if (string.IsNullOrWhiteSpace(user1) || string.IsNullOrWhiteSpace(user2))
            return BadRequest("Both user1 and user2 must be provided.");

        var q = _context.Tasks
            .AsNoTracking()
            .Where(t => t.IsAccepted && !string.IsNullOrEmpty(t.AcceptedBy))
            .Where(t =>
                (t.RequestedBy.ToLower() == user1.ToLower() && t.AcceptedBy!.ToLower() == user2.ToLower()) ||
                (t.RequestedBy.ToLower() == user2.ToLower() && t.AcceptedBy!.ToLower() == user1.ToLower()))
            .OrderByDescending(t => t.CreatedAt);

        var latest = await q.FirstOrDefaultAsync();
        if (latest is null) return NotFound("Ingen accepteret task findes mellem de to brugere.");
        return Ok(new { latest.Id, latest.CreatedAt });
    }
}
