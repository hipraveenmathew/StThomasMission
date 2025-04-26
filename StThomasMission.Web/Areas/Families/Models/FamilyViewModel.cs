using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150, ErrorMessage = "Family name cannot exceed 150 characters.")]
        public string FamilyName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid ward.")]
        public string Ward { get; set; } = string.Empty;

        public string TemporaryID { get; set; } = string.Empty;

        public bool IsRegistered { get; set; }

        [StringLength(8, ErrorMessage = "Church Registration Number must be 8 characters long.", MinimumLength = 8)]
        [RegularExpression(@"^10802\d{4}$", ErrorMessage = "Church Registration Number must be in format '10802XXXX'.")]
        public string? ChurchRegistrationNumber { get; set; }

        [StringLength(8, ErrorMessage = "Temporary ID must be 8 characters long.", MinimumLength = 8)]
        [RegularExpression(@"^TMP-\d{4}$", ErrorMessage = "Temporary ID must be in format 'TMP-XXXX'.")]
        public string? TemporaryId { get; set; }

        public string Status { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Migrated to location cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }
    }
}