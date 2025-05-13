using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }
        public Family Family { get; set; } = null!;

        public string? UserId { get; set; }
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

        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters.")]
        public string? Contact { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
        public string? Role { get; set; }

        public DateTime? DateOfBaptism { get; set; }
        public DateTime? DateOfChrismation { get; set; }
        public DateTime? DateOfHolyCommunion { get; set; }
        public DateTime? DateOfDeath { get; set; }

        public DateTime? DateOfMarriage { get; set; }

        public Student? StudentProfile { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public string FullName => $"{FirstName} {LastName}";
    }
}