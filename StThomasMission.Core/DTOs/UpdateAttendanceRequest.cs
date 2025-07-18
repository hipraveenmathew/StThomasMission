using StThomasMission.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class UpdateAttendanceRequest
    {
        [Required]
        public AttendanceStatus Status { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}