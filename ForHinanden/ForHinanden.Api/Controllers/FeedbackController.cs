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
        // Opret ny feedback (kun én pr. device)
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FeedbackDto dto)
        {
            if (dto is null)
                return BadRequest(new { message = "Anmodningens indhold mangler." });

            if (string.IsNullOrWhiteSpace(dto.DeviceId))
                return BadRequest(new { message = "DeviceId er påkrævet." });

            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Bedømmelse skal være mellem 1 og 5." });

            var eksisterende = await _context.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.DeviceId == dto.DeviceId);

            if (eksisterende != null)
                return Conflict(new
                {
                    message = "Feedback findes allerede for denne enhed. Brug PATCH-endpointet /api/feedback/{deviceId} til at opdatere."
                });

            var feedback = new Feedback
            {
                DeviceId = dto.DeviceId.Trim(),
                Rating = dto.Rating,
                FeedbackText = string.IsNullOrWhiteSpace(dto.Feedback) ? null : dto.Feedback.Trim(),
                EmojiLabel = dto.EmojiLabel,
                ImprovementText = dto.ImprovementText,
                VolunteerOpinion = dto.VolunteerOpinion,
                CreatedAt = DateTime.UtcNow
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            Console.WriteLine($"📣 Ny feedback fra {feedback.DeviceId}: {feedback.Rating}★ ({feedback.EmojiLabel})");

            return CreatedAtAction(nameof(GetOne), new { deviceId = feedback.DeviceId }, feedback);
        }

        // GET: /api/feedback
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

            if (feedback is null)
                return NotFound(new { message = "Ingen feedback fundet for denne enhed." });

            return Ok(feedback);
        }

        // PATCH: /api/feedback/{deviceId}
        // Opdater rating, emojiLabel, improvementText eller volunteerOpinion
        [HttpPatch("{deviceId}")]
        public async Task<IActionResult> Patch(string deviceId, [FromBody] FeedbackDto dto)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest(new { message = "DeviceId er påkrævet." });

            if (dto is null)
                return BadRequest(new { message = "Anmodningens indhold mangler." });

            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.DeviceId == deviceId);
            if (feedback is null)
                return NotFound(new { message = "Feedback blev ikke fundet for denne enhed." });

            var ændret = false;

            if (dto.Rating >= 1 && dto.Rating <= 5 && dto.Rating != feedback.Rating)
            {
                feedback.Rating = dto.Rating;
                ændret = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.Feedback))
            {
                feedback.FeedbackText = dto.Feedback.Trim();
                ændret = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.EmojiLabel))
            {
                feedback.EmojiLabel = dto.EmojiLabel.Trim();
                ændret = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.ImprovementText))
            {
                // 🟡 Behold tidligere tekst og tilføj ny linje
                if (!string.IsNullOrWhiteSpace(feedback.ImprovementText))
                    feedback.ImprovementText += "\n" + dto.ImprovementText.Trim();
                else
                    feedback.ImprovementText = dto.ImprovementText.Trim();

                ændret = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.VolunteerOpinion))
            {
                if (!string.IsNullOrWhiteSpace(feedback.VolunteerOpinion))
                    feedback.VolunteerOpinion += "\n" + dto.VolunteerOpinion.Trim();
                else
                    feedback.VolunteerOpinion = dto.VolunteerOpinion.Trim();

                ændret = true;
            }

            if (!ændret)
                return BadRequest(new { message = "Intet at opdatere." });

            feedback.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Feedback opdateret for {deviceId}");
            return Ok(feedback);
        }
        // ✅ GET: /api/feedback/{deviceId}/notify
        [HttpGet("{deviceId}/notify")]
        public async Task<IActionResult> Notify(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                return BadRequest("DeviceId er påkrævet.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);
            if (user is null)
                return NotFound($"Ingen bruger med DeviceId '{deviceId}'.");

            var fcmMessage = new FirebaseAdmin.Messaging.Message
            {
                Token = user.DeviceId, // DeviceId bruges som FCM-token
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = "📋 Vi vil gerne høre din mening!",
                    Body = "Har du 2 minutter til at dele din oplevelse i ForHinanden? ❤️"
                }
            };

            try
            {
                await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
                Console.WriteLine($"✅ Feedback invitation sendt til {deviceId}");
                return Ok(new { message = "Notifikation sendt." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Fejl ved FCM: {ex.Message}");
                return StatusCode(500, new { message = "Kunne ikke sende notifikation", error = ex.Message });
            }
        }
    }
}
