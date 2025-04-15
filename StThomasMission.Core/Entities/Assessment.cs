using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class Assessment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Assessment name is required.")]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Marks cannot be negative.")]
        public int Marks { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Total marks must be positive.")]
        public int TotalMarks { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsMajor { get; set; }

        public Student Student { get; set; } = null!;
    }
}