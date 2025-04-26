using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class GroupActivityViewModel
    {
        public string Grade { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid group ID.")]
        public int GroupId { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Activity name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Points must be a positive number.")]
        public int Points { get; set; }
    }
}