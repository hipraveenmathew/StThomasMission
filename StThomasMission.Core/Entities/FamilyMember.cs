using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class FamilyMember
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; } = string.Empty;

        public string? Relation { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Contact { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        public string? Role { get; set; } // e.g., Parent, added for SeedData compatibility

        public Family Family { get; set; } = null!;

        public Student? StudentProfile { get; set; }
    }
}