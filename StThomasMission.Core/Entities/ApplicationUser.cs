using Microsoft.AspNetCore.Identity;
using StThomasMission.Core.Entities;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward is required.")]
    public int WardId { get; set; } // Foreign key to Ward
    public Ward Ward { get; set; } = null!; // Navigation property

    public bool IsActive { get; set; } = true;

    [StringLength(250, ErrorMessage = "Profile image URL cannot exceed 250 characters.")]
    public string? ProfileImageUrl { get; set; }

    [StringLength(50)]
    public string? Designation { get; set; } // Optional title (e.g., Catechism Teacher, Admin)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }
}
