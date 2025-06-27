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
        [Range(2000, 9999)]
        public int AcademicYear { get; set; }

        // --- CORRECTED SECTION ---
        [Required(ErrorMessage = "Grade is required.")]
        public int GradeId { get; set; } // Foreign key to Grade table
        public Grade Grade { get; set; } = null!; // Navigation property

        [Range(0, double.MaxValue)]
        public double TotalMarks { get; set; }

        [Range(0, double.MaxValue)]
        public double ObtainedMarks { get; set; }

        public double Percentage => TotalMarks > 0 ? (ObtainedMarks / TotalMarks) * 100 : 0;

        [Required]
        public bool Passed { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        // --- Auditing Fields ---
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }
    }
}