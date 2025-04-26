using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class PromoteStudentsViewModel
    {
        [Required]
        [RegularExpression(@"^Year \d{1,2}$", ErrorMessage = "Grade must be in format 'Year X'.")]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [Range(2000, 9999, ErrorMessage = "Academic year must be a valid year.")]
        public int AcademicYear { get; set; }

        public string? SuccessMessage { get; set; }
    }
}