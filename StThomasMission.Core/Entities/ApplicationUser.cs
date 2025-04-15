using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        public string? Ward { get; set; } // Optional, for ParishAdmin/Teachers
    }
}