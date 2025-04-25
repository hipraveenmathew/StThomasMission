using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class StudentGroupActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        [Required]
        public int GroupActivityId { get; set; }
        public GroupActivity GroupActivity { get; set; } = null!;

        [Required]
        public DateTime ParticipationDate { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue, ErrorMessage = "Points cannot be negative.")]
        public int PointsEarned { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        [StringLength(100, ErrorMessage = "RecordedBy cannot exceed 100 characters.")]
        public string? RecordedBy { get; set; } // Optional for audit or staff traceability
    }
}
