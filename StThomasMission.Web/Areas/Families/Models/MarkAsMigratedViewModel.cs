using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class MarkAsMigratedViewModel
    {
        public int Id { get; set; } // Added Id property (replacing FamilyId)

        [Required]
        [StringLength(150, ErrorMessage = "Family name cannot exceed 150 characters.")]
        public string FamilyName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "Ward cannot exceed 100 characters.")]
        public string Ward { get; set; } = string.Empty;

        public bool IsRegistered { get; set; }

        [StringLength(8, ErrorMessage = "Church registration number cannot exceed 8 characters.")]
        public string? ChurchRegistrationNumber { get; set; }

        [StringLength(8, ErrorMessage = "Temporary ID cannot exceed 8 characters.")]
        public string? TemporaryID { get; set; } // Fixed to TemporaryID

        [Required]
        [StringLength(150, ErrorMessage = "Migrated to location cannot exceed 150 characters.")]
        public string MigratedTo { get; set; } = string.Empty;
    }
}