using StThomasMission.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public int FamilyMemberId { get; set; }
        public FamilyMember FamilyMember { get; set; } = null!;

        [Required]
        public int AcademicYear { get; set; }

        // --- CORRECTED SECTION ---
        [Required(ErrorMessage = "Grade is required.")]
        public int GradeId { get; set; } // Foreign key to Grade table
        public Grade Grade { get; set; } = null!; // Navigation property

        public int? GroupId { get; set; } // Nullable if a student might not have a group
        public Group? Group { get; set; } // Navigation property

        [StringLength(150)]
        public string? StudentOrganisation { get; set; }

        [Required]
        public StudentStatus Status { get; set; } = StudentStatus.Active;

        [StringLength(150, ErrorMessage = "Migration target cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }

        // --- Auditing Fields ---
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        // --- Navigation Properties for Related Data ---
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public ICollection<StudentGroupActivity> StudentGroupActivities { get; set; } = new List<StudentGroupActivity>();
        public ICollection<StudentAcademicRecord> AcademicRecords { get; set; } = new List<StudentAcademicRecord>();
    }
}