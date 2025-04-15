using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = "Catechism Class";

        public bool IsPresent { get; set; }

        public Student Student { get; set; } = null!;
    }
}