using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Ward name cannot exceed 100 characters.")]
        public string? Ward { get; set; } // Optional - for Teachers/Parish Admin

        // Future: Optional properties for better user management
        public bool IsActive { get; set; } = true; // For disabling user accounts without deletion
    }
}
