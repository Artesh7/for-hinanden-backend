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
    
    //et endpoint der tager sender og receiver
    // GET: api/messages/conversation?user1=Alice&user2=Bob
    [HttpGet("conversation")]
    public async Task<IActionResult> GetConversation([FromQuery] string user1, [FromQuery] string user2)
    {
        if (string.IsNullOrWhiteSpace(user1) || string.IsNullOrWhiteSpace(user2))
        {
            return BadRequest("Both user1 and user2 must be provided.");
        }

        var messages = await _context.Messages
            .Where(m => (m.Sender.ToLower() == user1.ToLower() && m.Receiver.ToLower() == user2.ToLower())
                        || (m.Sender.ToLower() == user2.ToLower() && m.Receiver.ToLower() == user1.ToLower()))
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }

}