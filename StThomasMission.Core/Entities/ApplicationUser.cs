using Microsoft.AspNetCore.Identity;

namespace StThomasMission.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Ward { get; set; } // Optional, for ParishAdmin/Teachers
    }
}