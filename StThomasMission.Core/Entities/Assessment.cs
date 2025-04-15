using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class Assessment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Assessment name is required.")]
        [MaxLength(150, ErrorMessage = "Assessment name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Marks cannot be negative.")]
        public int Marks { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Total marks must be positive.")]
        public int TotalMarks { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Assessment type is required.")]
        public AssessmentType Type { get; set; }  // ClassAssessment / SemesterExam

        public Student Student { get; set; } = null!;
    }
}
