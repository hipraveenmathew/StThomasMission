using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class Family
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Family name is required.")]
        public string FamilyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ward is required.")]
        public string Ward { get; set; } = string.Empty;

        public bool IsRegistered { get; set; }

        [RegularExpression(@"^10802\d{4}$", ErrorMessage = "Church registration number must be in format '10802XXXX'.")]
        public string? ChurchRegistrationNumber { get; set; } // e.g., "108020001"

        [RegularExpression(@"^TMP-\d{4}$", ErrorMessage = "Temporary ID must be in format 'TMP-XXXX'.")]
        public string? TemporaryID { get; set; } // e.g., "TMP-0001"

        public string? Status { get; set; } // Active, Migrated

        public string? MigratedTo { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();
    }
}