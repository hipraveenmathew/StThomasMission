using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public int FamilyMemberId { get; set; }
        public FamilyMember FamilyMember { get; set; } = null!;

        [Required]
        [Range(1, 12, ErrorMessage = "Academic year must be between 1 and 12.")]
        public int AcademicYear { get; set; }

        [Required(ErrorMessage = "Grade is required.")]
        [RegularExpression(@"^Year \d{1,2}$", ErrorMessage = "Grade must be in the format 'Year X'.")]
        [StringLength(20, ErrorMessage = "Grade cannot exceed 20 characters.")]
        public string Grade { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters.")]
        public string? Group { get; set; }

        [StringLength(150, ErrorMessage = "Organisation name cannot exceed 150 characters.")]
        public string? StudentOrganisation { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public StudentStatus Status { get; set; } = StudentStatus.Active; // Supports Inactive/Deleted

        [StringLength(150, ErrorMessage = "Migration target name cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }

        public DateTime? UpdatedDate { get; set; } // Tracks status changes

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
        public ICollection<StudentGroupActivity> GroupActivities { get; set; } = new List<StudentGroupActivity>();

        public double GPA => Assessments.Any(a => a.TotalMarks > 0)
            ? Math.Round(Assessments.Average(a => (double)a.Marks / a.TotalMarks) * 100, 2)
            : 0;

        public bool IsGraduated => Status == StudentStatus.Graduated;
        public bool IsMigrated => Status == StudentStatus.Migrated;
        public bool IsInactive => Status == StudentStatus.Inactive;
        public bool IsDeleted => Status == StudentStatus.Deleted;
    }
}
