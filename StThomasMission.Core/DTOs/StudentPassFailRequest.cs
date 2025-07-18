using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class StudentPassFailRequest
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public bool HasPassed { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}