using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Relation { get; set; } // Father, Mother, Son, Daughter, etc.

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        [MaxLength(20)]
        public string? Contact { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Role { get; set; } // For Identity / SeedData compatibility (e.g., Parent)

        public Family Family { get; set; } = null!;

        public Student? StudentProfile { get; set; }
    }
}
