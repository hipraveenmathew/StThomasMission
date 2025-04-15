using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class StudentGroupActivity
    {
        [Required]
        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        [Required]
        public int GroupActivityId { get; set; }

        public GroupActivity GroupActivity { get; set; } = null!;

        [Required]
        public DateTime ParticipationDate { get; set; } = DateTime.UtcNow;

        [Range(0, int.MaxValue, ErrorMessage = "Points cannot be negative.")]
        public int PointsEarned { get; set; } // Optional field for individual points in the activity
    }
}
