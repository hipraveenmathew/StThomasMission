using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class StudentEnrollmentViewModel
    {
        [Required]
        public int FamilyMemberId { get; set; }

        [Required]
        [RegularExpression(@"^Year \d{1,2}$", ErrorMessage = "Grade must be in format 'Year X'.")]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [Range(2000, 9999, ErrorMessage = "Academic year must be a valid year.")]
        public int AcademicYear { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid group.")]
        public int Group { get; set; }

        [StringLength(150, ErrorMessage = "Student organisation cannot exceed 150 characters.")]
        public string? StudentOrganisation { get; set; }
    }
}