using StThomasMission.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents a catechism student.
    /// </summary>
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public int FamilyMemberId { get; set; }
        public FamilyMember FamilyMember { get; set; } = null!;

        [Required]
        public int AcademicYear { get; set; }

        [Required(ErrorMessage = "Grade is required.")]
        public int GradeId { get; set; } // Foreign key to Grade
        public Grade Grade { get; set; } = null!;

        public int? GroupId { get; set; } // Nullable if a student isn't in a group
        public Group? Group { get; set; }

        [StringLength(150)]
        public string? StudentOrganisation { get; set; }

        [Required]
        public StudentStatus Status { get; set; } = StudentStatus.Active;

        [StringLength(150, ErrorMessage = "Migration destination cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }

        public bool IsDeleted { get; set; }

        // --- Auditing Fields ---
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        // --- Navigation Properties ---
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public ICollection<StudentAcademicRecord> AcademicRecords { get; set; } = new List<StudentAcademicRecord>();
    }
}