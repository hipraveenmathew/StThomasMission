using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class Family
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Family name is required.")]
        [MaxLength(150)]
        public string FamilyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ward is required.")]
        [MaxLength(100)]
        public string Ward { get; set; } = string.Empty;

        public bool IsRegistered { get; set; }

        [RegularExpression(@"^10802\d{4}$", ErrorMessage = "Church registration number must be in format '10802XXXX'.")]
        public string? ChurchRegistrationNumber { get; set; }

        [RegularExpression(@"^TMP-\d{4}$", ErrorMessage = "Temporary ID must be in format 'TMP-XXXX'.")]
        public string? TemporaryID { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public FamilyStatus Status { get; set; } = FamilyStatus.Active;

        public string? MigratedTo { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();
    }
}
