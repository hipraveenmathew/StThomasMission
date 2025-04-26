using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class EditStudentViewModel
    {
        public int Id { get; set; }

        public int FamilyMemberId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^Year \d{1,2}$", ErrorMessage = "Grade must be in format 'Year X'.")]
        public string Grade { get; set; }

        [Required]
        [Range(2000, 9999, ErrorMessage = "Academic year must be a valid year.")]
        public int AcademicYear { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid group ID.")]
        public int GroupId { get; set; }

        [StringLength(150, ErrorMessage = "Student organisation cannot exceed 150 characters.")]
        public string StudentOrganisation { get; set; }

        [Required]
        public string Status { get; set; }
    }
}