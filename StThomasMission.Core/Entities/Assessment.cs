using System;
using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class Assessment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; } // Foreign key to Student
        public Student Student { get; set; } = null!; // Navigation property

        [Required(ErrorMessage = "Assessment name is required.")]
        [StringLength(150, ErrorMessage = "Assessment name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Marks cannot be negative.")]
        public double Marks { get; set; } // Already updated to double

        [Range(1, double.MaxValue, ErrorMessage = "Total marks must be positive.")]
        public double TotalMarks { get; set; } // Already updated to double

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }


        [Required]
        public AssessmentType Type { get; set; } = AssessmentType.ClassAssessment; // Reverted to Type

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public double Percentage => TotalMarks > 0 ? Math.Round((Marks / TotalMarks) * 100, 2) : 0;

        // Suggested index: (StudentId, Date)
    }
}