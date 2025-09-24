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
            if (dto.Stars < 1 || dto.Stars > 5) return BadRequest("Stars skal være mellem 1 og 5.");

            var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == dto.TaskId);
            if (task is null) return NotFound("Task findes ikke.");

            if (!task.IsAccepted || string.IsNullOrWhiteSpace(task.AcceptedBy))
                return BadRequest("Du kan kun give rating på accepterede opgaver.");

            // Rating må kun være mellem de to parter
            var req = task.RequestedBy.Trim();
            var acc = task.AcceptedBy.Trim();
            var from = dto.RatedBy.Trim();
            var to = dto.ToUserId.Trim();

            bool validPair = (from == req && to == acc) || (from == acc && to == req);
            if (!validPair)
                return Forbid("Rating må kun ske mellem task-ejer og den accepterede hjælper.");

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

        // GET /api/ratings/user/{userId}   (alle ratings MODTAGEREN har fået)
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(string userId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.ToUserId.ToLower() == userId.ToLower())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ratings);
        }

        // GET /api/ratings/user/{userId}/given   (alle ratings som userId HAR GIVET)
        [HttpGet("user/{userId}/given")]
        public async Task<IActionResult> GetGivenByUser(string userId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.RatedBy.ToLower() == userId.ToLower())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ratings);
        }

        // GET /api/ratings/user/{userId}/average
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
