using ForHinanden.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using ForHinanden.Api.Data;

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
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] FeedbackDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DeviceId))
                return BadRequest(new { message = "DeviceId is required" });

            // Opret en ny feedback-model og gem i databasen
            var feedback = new Feedback
            {
                DeviceId = dto.DeviceId,
                FeedbackText = dto.Feedback,
                Timestamp = dto.Timestamp
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            Console.WriteLine($"📣 Feedback fra {dto.DeviceId}: {dto.Feedback} ({dto.Timestamp})");

            return Ok(new { message = "Feedback received" });
        }

        // (Valgfrit) GET: /api/feedback
        // Til test eller admin-dashboard — viser alle feedbacks
        [HttpGet]
        public IActionResult GetAll()
        {
            var feedbacks = _context.Feedbacks
                .OrderByDescending(f => f.Timestamp)
                .ToList();

            return Ok(feedbacks);
        }
    }
}