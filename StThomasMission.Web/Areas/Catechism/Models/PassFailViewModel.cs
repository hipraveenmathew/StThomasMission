using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class PassFailViewModel
    {
        public int StudentId { get; set; }

        [Required]
        [Range(2000, 9999, ErrorMessage = "Academic year must be a valid year.")]
        public int AcademicYear { get; set; }

        [Range(0, 100, ErrorMessage = "Pass threshold must be between 0 and 100.")]
        public double PassThreshold { get; set; } = 50.0;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }
}