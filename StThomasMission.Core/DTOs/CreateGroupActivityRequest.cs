using StThomasMission.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class CreateGroupActivityRequest
    {
        [Required]
        public int GroupId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 10000)]
        public int Points { get; set; }

        public ActivityStatus Status { get; set; } = ActivityStatus.Active;
    }
}