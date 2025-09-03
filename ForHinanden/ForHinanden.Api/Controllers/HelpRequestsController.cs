
using Microsoft.AspNetCore.Mvc;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;


namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelpRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public HelpRequestsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HelpRequest>>> GetAll()
        => await _context.HelpRequests.ToListAsync();

    [HttpPost]
    public async Task<ActionResult<HelpRequest>> Create([FromBody] CreateHelpRequestDto dto)
    {
        var request = new HelpRequest
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            RequestedBy = dto.RequestedBy,
            IsAccepted = false,      // default
            AcceptedBy = null        // default
        };

        _context.HelpRequests.Add(request);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = request.Id }, request);
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptHelp(Guid id, [FromBody] AcceptHelpDto body)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.AcceptedBy))
            return BadRequest("acceptedBy er påkrævet.");

        var request = await _context.HelpRequests.FindAsync(id);
        if (request == null) return NotFound();

        if (request.IsAccepted) return BadRequest("Opgaven er allerede accepteret.");

        request.IsAccepted = true;
        request.AcceptedBy = body.AcceptedBy;

        await _context.SaveChangesAsync();
        return Ok(request);
    }

}
