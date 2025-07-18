using StThomasMission.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents an individual member belonging to a family.
    /// </summary>
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }
        public Family Family { get; set; } = null!;

        public string? UserId { get; set; } // Link to an ApplicationUser account
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Baptismal name cannot exceed 100 characters.")]
        public string? BaptismalName { get; set; }

        [Required]
        public FamilyMemberRole Relation { get; set; } = FamilyMemberRole.Other;

        [Required]
        public DateTime DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters.")]
        public string? Contact { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        public bool IsDeleted { get; set; }

        // --- Auditing Fields ---
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        [NotMapped] // This property is computed and not stored in the database.
        public string FullName => $"{FirstName} {LastName}";

        // --- Navigation Properties ---
        public Student? StudentProfile { get; set; }
    }
}