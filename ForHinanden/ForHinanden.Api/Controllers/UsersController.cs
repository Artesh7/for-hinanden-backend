using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ForHinanden.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context) => _context = context;

    // POST /api/users  (opret ny bruger; fejler hvis deviceId findes)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest("DeviceId is required.");

        var exists = await _context.Users.AnyAsync(u => u.DeviceId == dto.DeviceId);
        if (exists) return Conflict("User with this deviceId already exists.");

        var user = new User
        {
            DeviceId  = dto.DeviceId,
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            City      = dto.City,
            ProfilePictureUrl = dto.ProfilePictureUrl // kan være null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Created($"/api/users/by-deviceId?deviceId={Uri.EscapeDataString(user.DeviceId)}", user);
    }

    // PUT /api/users  (opdater eksisterende; bruger deviceId fra body)
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest("DeviceId is required.");

        var user = await _context.Users.FindAsync(dto.DeviceId);
        if (user is null) return NotFound("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName  = dto.LastName;
        user.City      = dto.City;

        if (dto.ProfilePictureUrl != null)
            user.ProfilePictureUrl = string.IsNullOrWhiteSpace(dto.ProfilePictureUrl) ? null : dto.ProfilePictureUrl;

        await _context.SaveChangesAsync();
        return Ok(user);
    }

    // GET /api/users/{deviceId}
    [HttpGet("{deviceId}")]
    public async Task<IActionResult> GetByDeviceId(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest("DeviceId is required.");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.DeviceId == deviceId);

        return user is null ? NotFound() : Ok(user);
    }

    // POST /api/users/photo  (upload/erstat profilbillede – deviceId + file i form-data)
    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadPhoto([FromForm] UploadUserPhotoForm form)
    {
        if (form is null || string.IsNullOrWhiteSpace(form.deviceId))
            return BadRequest("deviceId is required.");

        var user = await _context.Users.FindAsync(form.deviceId);
        if (user is null) return NotFound("User not found.");

        var file = form.file;
        if (file is null || file.Length == 0)
            return BadRequest("No file received.");

        var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        var contentType = file.ContentType.ToLower();
        if (!allowed.Contains(contentType))
            return BadRequest("Only JPG, PNG or WEBP allowed.");

        var ext = contentType switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".bin"
        };

        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", "users");
        Directory.CreateDirectory(uploadDir);

        // Slet tidligere fil hvis den er lokalt gemt under /uploads/users/
        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl) &&
            user.ProfilePictureUrl!.StartsWith("/uploads/users/", StringComparison.OrdinalIgnoreCase))
        {
            var oldPath = Path.Combine(webRoot, user.ProfilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        var fileName = $"{user.DeviceId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
        var fullPath = Path.Combine(uploadDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var publicUrl = $"/uploads/users/{fileName}";
        user.ProfilePictureUrl = publicUrl;
        await _context.SaveChangesAsync();

        return Ok(new { url = publicUrl });
    }

    // POST /api/users/photo/delete  (slet profilbillede via body med deviceId)
    [HttpDelete("photo")]
    public async Task<IActionResult> DeletePhoto([FromBody] DeleteUserPhotoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest("DeviceId is required.");

        var user = await _context.Users.FindAsync(dto.DeviceId);
        if (user is null) return NotFound("User not found.");

        var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl) &&
            user.ProfilePictureUrl!.StartsWith("/uploads/users/", StringComparison.OrdinalIgnoreCase))
        {
            var oldPath = Path.Combine(webRoot, user.ProfilePictureUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        user.ProfilePictureUrl = null;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
