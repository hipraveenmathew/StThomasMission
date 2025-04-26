using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class StudentAcademicRecord
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; } // Foreign key to Student
        public Student Student { get; set; } = null!; // Navigation property

        [Required]
        public int AcademicYear { get; set; }

        [Required]
        public string Grade { get; set; } = string.Empty;

        [Required]
        public bool Passed { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}