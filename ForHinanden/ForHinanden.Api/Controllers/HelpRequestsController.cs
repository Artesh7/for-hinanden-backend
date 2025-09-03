
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
    public async Task<ActionResult<HelpRequest>> Create(HelpRequest request)
    {
        _context.HelpRequests.Add(request);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = request.Id }, request);
    }

    [HttpPost("{id}/offer")]
    public async Task<IActionResult> OfferHelp(Guid id, [FromBody] string offeredBy)
    {
        var request = await _context.HelpRequests.FindAsync(id);
        if (request == null) return NotFound();
        if (request.OfferedBy != null) return BadRequest("Hjælp allerede tilbudt.");

        request.OfferedBy = offeredBy;
        await _context.SaveChangesAsync();
        return Ok(request);
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptHelp(Guid id)
    {
        var request = await _context.HelpRequests.FindAsync(id);
        if (request == null) return NotFound();
        if (string.IsNullOrEmpty(request.OfferedBy)) return BadRequest("Ingen har tilbudt hjælp endnu.");

        request.IsAccepted = true;
        await _context.SaveChangesAsync();
        return Ok(request);
    }
}
