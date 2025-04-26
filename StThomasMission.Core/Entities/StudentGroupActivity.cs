using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class StudentGroupActivity
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; } // Foreign key to Student
        public Student Student { get; set; } = null!; // Navigation property

        [Required]
        public int GroupActivityId { get; set; } // Foreign key to GroupActivity
        public GroupActivity GroupActivity { get; set; } = null!; // Navigation property

        [Required]
        public DateTime ParticipationDate { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue, ErrorMessage = "Points cannot be negative.")]
        public int PointsEarned { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        [StringLength(100, ErrorMessage = "RecordedBy cannot exceed 100 characters.")]
        public string? RecordedBy { get; set; }

        // Suggested index: (StudentId, GroupActivityId)
    }
}