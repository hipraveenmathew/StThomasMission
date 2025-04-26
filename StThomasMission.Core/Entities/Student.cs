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
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        [Required]
        [Range(2000, 9999, ErrorMessage = "Academic year must be a valid year.")]
        public int AcademicYear { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Grade cannot exceed 20 characters.")]
        public string Grade { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Student organisation cannot exceed 150 characters.")]
        public string? StudentOrganisation { get; set; }

        [Required]
        public StudentStatus Status { get; set; }

        [StringLength(150, ErrorMessage = "Migrated to cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public List<Attendance> Attendances { get; set; } = new();
        public List<Assessment> Assessments { get; set; } = new();
        public List<StudentGroupActivity> GroupActivities { get; set; } = new();
    }
}