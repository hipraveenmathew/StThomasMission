using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents a user of the application, extending the base IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// The ward this user belongs to. Nullable for system-level users.
        /// </summary>
        public int? WardId { get; set; } // Foreign key to Ward
        public Ward? Ward { get; set; } // Navigation property

        public bool IsActive { get; set; } = true;

        [StringLength(250, ErrorMessage = "Profile image URL cannot exceed 250 characters.")]
        public string? ProfileImageUrl { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; } // e.g., Catechism Teacher, Admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}