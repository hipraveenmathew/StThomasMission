using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid ward.")]
        public int WardId { get; set; }

        [StringLength(50)]
        public string? Designation { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}