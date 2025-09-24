using System;
using System.Threading.Tasks;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/profiles")]
public class UserProfileController : ControllerBase
{
    private readonly AppDbContext _context;
    public UserProfileController(AppDbContext context) => _context = context;

    // GET /api/profiles/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _context.UserProfiles.FindAsync(id);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    // POST /api/profiles
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserProfileDto dto)
    {
        var profile = new UserProfile
        {
            UserName = dto.UserName,
            FullName = dto.FullName,
            ProfilePictureUrl = dto.ProfilePictureUrl,
            Bio = dto.Bio
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return Created($"/api/profiles/{profile.Id}", profile);
    }

    // PUT /api/profiles/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateUserProfileDto dto)
    {
        var profile = await _context.UserProfiles.FindAsync(id);
        if (profile == null) return NotFound();

        profile.FullName = dto.FullName;
        profile.ProfilePictureUrl = dto.ProfilePictureUrl;
        profile.Bio = dto.Bio;

        await _context.SaveChangesAsync();
        return Ok(profile);
    }
}