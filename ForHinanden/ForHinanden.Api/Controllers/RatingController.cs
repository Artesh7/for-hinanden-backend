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
            var rating = new Rating
            {
                UserProfileId = dto.UserProfileId,
                RatedBy = dto.RatedBy,
                Stars = dto.Stars,
                Comment = dto.Comment
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return Created($"/api/ratings/{rating.Id}", rating);
        }

        // GET /api/ratings/user/{userId}
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetForUser(Guid userId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.UserProfileId == userId)
                .ToListAsync();

            return Ok(ratings);
        }
    }
}