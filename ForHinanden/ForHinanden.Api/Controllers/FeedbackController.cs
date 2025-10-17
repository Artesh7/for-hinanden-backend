using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ForHinanden.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FeedbackController(AppDbContext context)
        {
            _context = context;
        }

        // POST: /api/feedback
        // Opret en ny feedback (1 pr. device). Bruger bedømmelse 1..5 + valgfri tekst.
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FeedbackDto dto)
        {
            if (dto is null) return BadRequest(new { message = "Anmodningens indhold mangler." });
            if (string.IsNullOrWhiteSpace(dto.DeviceId))
                return BadRequest(new { message = "DeviceId er påkrævet." });
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Bedømmelse skal være mellem 1 og 5." });

            var eksisterende = await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.DeviceId == dto.DeviceId);

            if (eksisterende != null)
                return Conflict(new { message = "Feedback findes allerede for denne enhed. Brug PATCH-endpointet /api/feedback/{deviceId} til at opdatere." });

            var feedback = new Feedback
            {
                DeviceId = dto.DeviceId.Trim(),
                Rating = dto.Rating,
                FeedbackText = string.IsNullOrWhiteSpace(dto.Feedback) ? null : dto.Feedback.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            Console.WriteLine($"📣 App-feedback fra {feedback.DeviceId}: {feedback.Rating}★ {(feedback.FeedbackText ?? "")}");

            // Returnér oprettet ressource
            return CreatedAtAction(nameof(GetOne), new { deviceId = feedback.DeviceId }, feedback);
        }

        // GET: /api/feedback
        // (typisk til admin/overblik)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var feedbacks = await _context.Feedbacks
                .AsNoTracking()
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Ok(feedbacks);
        }

        // GET: /api/feedback/{deviceId}
        [HttpGet("{deviceId}")]
        public async Task<IActionResult> GetOne(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { message = "DeviceId er påkrævet." });

            var feedback = await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.DeviceId == deviceId);

            if (feedback is null) return NotFound(new { message = "Ingen feedback fundet for denne enhed." });
            return Ok(feedback);
        }

        // PATCH: /api/feedback/{deviceId}
        // Opdater enten bedømmelse, tekst – eller begge.
        [HttpPatch("{deviceId}")]
        public async Task<IActionResult> Patch(string deviceId, [FromBody] UpdateFeedbackDto dto)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { message = "DeviceId er påkrævet." });
            if (dto is null) return BadRequest(new { message = "Anmodningens indhold mangler." });

            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.DeviceId == deviceId);
            if (feedback is null) return NotFound(new { message = "Feedback blev ikke fundet for denne enhed." });

            var ændret = false;

            if (dto.Rating.HasValue)
            {
                if (dto.Rating.Value < 1 || dto.Rating.Value > 5)
                    return BadRequest(new { message = "Bedømmelse skal være mellem 1 og 5." });

                feedback.Rating = dto.Rating.Value;
                ændret = true;
            }

            if (dto.Feedback != null)
            {
                feedback.FeedbackText = string.IsNullOrWhiteSpace(dto.Feedback) ? null : dto.Feedback.Trim();
                ændret = true;
            }

            if (!ændret)
                return BadRequest(new { message = "Intet at opdatere. Angiv bedømmelse og/eller feedbacktekst." });

            feedback.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(feedback);
        }
    }
}