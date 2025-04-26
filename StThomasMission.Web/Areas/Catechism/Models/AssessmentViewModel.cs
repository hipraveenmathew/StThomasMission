using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class AssessmentViewModel
    {
        public int StudentId { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Assessment name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Marks must be a positive number.")]
        public double Marks { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Total marks must be a positive number.")]
        public double TotalMarks { get; set; }

        public bool IsMajor { get; set; }
    }
}