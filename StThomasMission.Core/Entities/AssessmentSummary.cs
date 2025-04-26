using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class AssessmentSummary
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        [Required]
        [Range(2000, 9999, ErrorMessage = "Academic year must be a valid year.")]
        public int AcademicYear { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Grade cannot exceed 20 characters.")]
        public string Grade { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Total marks cannot be negative.")]
        public double TotalMarks { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Obtained marks cannot be negative.")]
        public double ObtainedMarks { get; set; }

        public double Percentage => TotalMarks > 0 ? (ObtainedMarks / TotalMarks) * 100 : 0;

        [Required]
        public bool Passed { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }
    }
}