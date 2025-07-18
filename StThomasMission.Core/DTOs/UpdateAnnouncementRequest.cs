using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class UpdateAnnouncementRequest
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime PostedDate { get; set; }

        public bool IsActive { get; set; }
    }
}