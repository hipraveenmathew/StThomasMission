using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public int FamilyMemberId { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "Academic year must be between 1 and 12.")]
        public int AcademicYear { get; set; }  // Year number (1-12)

        [Required(ErrorMessage = "Grade is required.")]
        [RegularExpression(@"^Year \d{1,2}$", ErrorMessage = "Grade must be in format 'Year X'.")]
        [MaxLength(20)]
        public string Grade { get; set; } = string.Empty; // e.g., Year 1

        [MaxLength(100)]
        public string? Group { get; set; } // St. Peter Group etc.

        [MaxLength(150)]
        public string? StudentOrganisation { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public StudentStatus Status { get; set; } = StudentStatus.Active;

        [MaxLength(150)]
        public string? MigratedTo { get; set; }

        public FamilyMember FamilyMember { get; set; } = null!;

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
