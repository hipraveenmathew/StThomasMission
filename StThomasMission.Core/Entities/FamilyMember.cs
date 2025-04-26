using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; } // Foreign key to Family
        public Family Family { get; set; } = null!; // Navigation property

        public string? UserId { get; set; } // Foreign key to ApplicationUser
        public ApplicationUser? User { get; set; } // Navigation property

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public FamilyMemberRole Relation { get; set; } = FamilyMemberRole.Other;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters.")]
        public string? Contact { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
        public string? Role { get; set; } // e.g., Catechism Student

        public Student? StudentProfile { get; set; } // Navigation to Student

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!; // Optimistic concurrency

        public string FullName => $"{FirstName} {LastName}";

        // Suggested index: FamilyId, UserId
    }
}