using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class FeeReminderViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid family ID.")]
        public int FamilyId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Fee details cannot exceed 1000 characters.")]
        public string FeeDetails { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
    }
}