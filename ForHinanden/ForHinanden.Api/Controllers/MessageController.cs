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

    // POST: api/messages
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMessageDto dto)
    {
        var message = new Message
        {
            TaskId = dto.TaskId,
            Sender = dto.Sender,
            Receiver = dto.Receiver,
            Content = dto.Content
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Created($"/api/messages/{message.Id}", message);
    }
}