using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;              // <-- Cloudinary
using CloudinaryDotNet.Actions;      // <-- Cloudinary

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

        if (dto.Bio is not null && dto.Bio.Length > 500)
            return BadRequest("Bio must be ≤ 500 characters.");

        var exists = await _context.Users.AnyAsync(u => u.DeviceId == dto.DeviceId);
        if (exists) return Conflict("User with this deviceId already exists.");

        var user = new User
        {
            DeviceId  = dto.DeviceId,
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            City      = dto.City,
            ProfilePictureUrl = dto.ProfilePictureUrl, // kan være null
            Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Returner Location = GET /api/users/{deviceId}
        return CreatedAtAction(nameof(GetByDeviceId), new { deviceId = user.DeviceId }, user);
    }

    // PUT /api/users  (opdater eksisterende; bruger deviceId fra body)
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest("DeviceId is required.");

        if (dto.Bio is not null && dto.Bio.Length > 500)
            return BadRequest("Bio must be ≤ 500 characters.");

        var user = await _context.Users.FindAsync(dto.DeviceId);
        if (user is null) return NotFound("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName  = dto.LastName;
        user.City      = dto.City;

        // null = ingen ændring, "" = sæt til null
        if (dto.ProfilePictureUrl != null)
            user.ProfilePictureUrl = string.IsNullOrWhiteSpace(dto.ProfilePictureUrl) ? null : dto.ProfilePictureUrl;

        // null = ingen ændring, "" = sæt til null
        if (dto.Bio != null)
            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();

        await _context.SaveChangesAsync();
        return Ok(user);
    }

    // PATCH /api/users/bio  (opdater KUN bio)
    [HttpPatch("bio")]
    public async Task<IActionResult> UpdateBio([FromBody] UpdateUserBioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest("DeviceId is required.");

        if (dto.Bio is not null && dto.Bio.Length > 500)
            return BadRequest("Bio must be ≤ 500 characters.");

        var user = await _context.Users.FindAsync(dto.DeviceId);
        if (user is null) return NotFound("User not found.");

        user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
        await _context.SaveChangesAsync();

        return Ok(new { deviceId = user.DeviceId, bio = user.Bio });
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
    // Nu: uploader til Cloudinary i stedet for lokal disk.
    [HttpPost("photo")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 10_000_000)]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadPhoto(
        [FromForm] UploadUserPhotoForm form,
        [FromServices] Cloudinary cloudinary) // <-- injiceret fra Program.cs
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

        // Upload til Cloudinary
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "for-hinanden/users", // valgfri mappe i Cloudinary
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != System.Net.HttpStatusCode.OK || result.SecureUrl == null)
            return StatusCode(500, $"Cloudinary upload failed ({result.StatusCode}).");

        // Gem fuld Cloudinary URL i DB
        user.ProfilePictureUrl = result.SecureUrl.ToString();
        await _context.SaveChangesAsync();

        return Ok(new { url = user.ProfilePictureUrl });
    }

    // DELETE /api/users/photo  (slet profilbillede)
    // Understøtter:
    //  - Cloudinary URL: forsøger at slette via public_id
    //  - Legacy lokal fil: sletter fra wwwroot/uploads/users
    [HttpDelete("photo")]
    public async Task<IActionResult> DeletePhoto(
        [FromBody] DeleteUserPhotoDto dto,
        [FromServices] Cloudinary cloudinary)
    {
        if (string.IsNullOrWhiteSpace(dto.DeviceId))
            return BadRequest("DeviceId is required.");

        var user = await _context.Users.FindAsync(dto.DeviceId);
        if (user is null) return NotFound("User not found.");

        if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
        {
            var url = user.ProfilePictureUrl!.Trim();

            // 1) Hvis det er Cloudinary, forsøg at slette via public_id
            if (url.Contains("res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
            {
                var publicId = TryExtractCloudinaryPublicId(url);
                if (!string.IsNullOrWhiteSpace(publicId))
                {
                    try
                    {
                        var del = await cloudinary.DestroyAsync(new DeletionParams(publicId)
                        {
                            ResourceType = ResourceType.Image
                        });
                        // del.Result kan være "ok" / "not found" osv. – vi ignorerer fejl her.
                    }
                    catch
                    {
                        // Ignorer slettefejl – vi rydder uanset i DB.
                    }
                }
            }
            // 2) Legacy lokal sti: /uploads/users/...
            else if (url.StartsWith("/uploads/users/", StringComparison.OrdinalIgnoreCase))
            {
                var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var oldPath = Path.Combine(webRoot, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { /* ignore */ }
                }
            }
        }

        user.ProfilePictureUrl = null;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Hjælper: udtræk Cloudinary public_id fra en SecureUrl
    private static string? TryExtractCloudinaryPublicId(string fullUrl)
    {
        try
        {
            var uri = new Uri(fullUrl);
            // typisk: /<resType>/upload/v1695922334/for-hinanden/users/filnavn.jpg
            var parts = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

            var uploadIdx = parts.FindIndex(p => string.Equals(p, "upload", StringComparison.OrdinalIgnoreCase));
            if (uploadIdx < 0 || uploadIdx + 1 >= parts.Count) return null;

            var after = parts.Skip(uploadIdx + 1).ToList();
            if (!after.Any()) return null;

            // drop evt. versionsled som "v1695922334"
            if (after[0].Length > 1 && after[0][0] == 'v' && long.TryParse(after[0].Substring(1), out _))
                after = after.Skip(1).ToList();

            if (!after.Any()) return null;

            var last = after[^1];
            var nameNoExt = Path.GetFileNameWithoutExtension(last);
            var path = string.Join("/", after.Take(after.Count - 1).Concat(new[] { nameNoExt }));
            return path; // fx "for-hinanden/users/filnavn"
        }
        catch
        {
            return null;
        }
    }
}
 