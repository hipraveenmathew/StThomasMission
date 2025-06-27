using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class StudentAcademicRecord
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        [Required]
        public int AcademicYear { get; set; }

        // --- CORRECTED SECTION ---
        [Required(ErrorMessage = "Grade is required.")]
        public int GradeId { get; set; } // Foreign key to Grade table
        public Grade Grade { get; set; } = null!; // Navigation property

        [Required]
        public bool Passed { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        // --- Auditing Fields ---
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }
    }
}