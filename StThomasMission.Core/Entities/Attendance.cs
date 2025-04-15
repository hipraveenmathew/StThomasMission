using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(250)]
        public string Description { get; set; } = "Catechism Class";

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public Student Student { get; set; } = null!;
    }
}
