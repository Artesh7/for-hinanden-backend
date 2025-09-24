﻿using System;
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
        var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.TaskId);
        if (task is null) return NotFound("Task findes ikke.");

        if (!task.IsAccepted || string.IsNullOrWhiteSpace(task.AcceptedBy))
            return BadRequest("Du kan ikke sende beskeder før en anmodning er accepteret.");

        // Kun mellem RequestedBy og AcceptedBy
        var s = dto.Sender?.Trim();
        var r = dto.Receiver?.Trim();
        var req = task.RequestedBy.Trim();
        var acc = task.AcceptedBy.Trim();

        bool pairOk = (s == req && r == acc) || (s == acc && r == req);
        if (!pairOk) return Forbid("Beskeder må kun sendes mellem task-ejer og den accepterede hjælper.");

        var message = new Message
        {
            TaskId = dto.TaskId,
            Sender = s!,
            Receiver = r!,
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
            .Where(m => (m.Sender.ToLower() == user1.ToLower() && m.Receiver.ToLower() == user2.ToLower())
                        || (m.Sender.ToLower() == user2.ToLower() && m.Receiver.ToLower() == user1.ToLower()))
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }
}
