using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForHinanden.Api.Models.Dtos
{
    public class UpdateTaskDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public string RequestedBy { get; set; } = null!;

        [Required]
        public Guid CityId { get; set; }

        [Required]
        public Guid PriorityOptionId { get; set; }

        [Required]
        public Guid DurationOptionId { get; set; }

        public List<Guid>? CategoryIds { get; set; }
    }
}