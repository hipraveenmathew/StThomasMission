using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public int FamilyMemberId { get; set; }

        [Required]
        public int AcademicYear { get; set; }

        [Required(ErrorMessage = "Grade is required.")]
        [RegularExpression(@"^Year \d{1,2}$", ErrorMessage = "Grade must be in format 'Year X'.")]
        public string Grade { get; set; } = string.Empty; // e.g., Year 1

        public string? Group { get; set; }

        public string? StudentOrganisation { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "Active"; // Active, Graduated, Migrated

        public string? MigratedTo { get; set; }

        public FamilyMember FamilyMember { get; set; } = null!;

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}