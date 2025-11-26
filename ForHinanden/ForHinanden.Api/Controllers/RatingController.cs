using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ForHinanden.Api.Data;
using ForHinanden.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ForHinanden.Api.Controllers
{
    [ApiController]
    [Route("api/ratings")]
    public class RatingController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RatingController(AppDbContext context) => _context = context;

        // POST /api/ratings
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRatingDto dto)
        {
            if (dto is null) return BadRequest("Body is required.");
            if (dto.TaskId == Guid.Empty) return BadRequest("taskId is required.");
            if (string.IsNullOrWhiteSpace(dto.ToUserId)) return BadRequest("toUserId is required.");
            if (string.IsNullOrWhiteSpace(dto.RatedBy)) return BadRequest("ratedBy is required.");
            if (dto.Stars < 1 || dto.Stars > 5) return BadRequest("Stars skal være mellem 1 og 5.");

            var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.TaskId);
            if (task is null) return NotFound("Task findes ikke.");

            if (!task.IsAccepted || string.IsNullOrWhiteSpace(task.AcceptedBy))
                return StatusCode(403, "Du kan kun give rating på accepterede opgaver.");

            var req = (task.RequestedBy ?? string.Empty).Trim();
            var acc = (task.AcceptedBy ?? string.Empty).Trim();
            var from = dto.RatedBy.Trim();
            var to = dto.ToUserId.Trim();

            bool validPair =
                (from.Equals(req, StringComparison.OrdinalIgnoreCase) && to.Equals(acc, StringComparison.OrdinalIgnoreCase)) ||
                (from.Equals(acc, StringComparison.OrdinalIgnoreCase) && to.Equals(req, StringComparison.OrdinalIgnoreCase));

            if (!validPair)
                return StatusCode(403, "Rating må kun ske mellem task-ejer og den accepterede hjælper.");

            var rating = new Rating
            {
                TaskId = dto.TaskId,
                ToUserId = to,
                RatedBy = from,
                Stars = dto.Stars,
                Comment = dto.Comment
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return Created($"/api/ratings/{rating.Id}", rating);
        }

        // (uændret)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(string userId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.ToUserId.ToLower() == userId.ToLower())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ratings);
        }

        [HttpGet("user/{userId}/given")]
        public async Task<IActionResult> GetGivenByUser(string userId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.RatedBy.ToLower() == userId.ToLower())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ratings);
        }

        [HttpGet("user/{userId}/average")]
        public async Task<IActionResult> GetAverageForUser(string userId)
        {
            var q = _context.Ratings.Where(r => r.ToUserId.ToLower() == userId.ToLower());

            var count = await q.CountAsync();
            if (count == 0) return Ok(new { userId, average = (double?)null, count = 0 });

            var avg = await q.AverageAsync(r => r.Stars);
            return Ok(new { userId, average = Math.Round(avg, 2), count });
        }
    }
}
