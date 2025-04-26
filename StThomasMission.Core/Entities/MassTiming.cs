using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class MassTiming
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Day is required.")]
        [StringLength(20, ErrorMessage = "Day cannot exceed 20 characters.")]
        public string Day { get; set; } = string.Empty;

        [Required]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string Location { get; set; } = string.Empty;

        [Required]
        public MassType Type { get; set; } = MassType.Regular;

        [Required]
        public DateTime WeekStartDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        // Suggested index: WeekStartDate
    }
}