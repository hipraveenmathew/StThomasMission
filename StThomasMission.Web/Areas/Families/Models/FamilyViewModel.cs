using StThomasMission.Core.Enums;
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

        public bool IsRegistered { get; set; }

        [StringLength(9, MinimumLength = 9, ErrorMessage = "Church Registration Number must be 9 characters long.")]
        [RegularExpression(@"^10802\d{4}$", ErrorMessage = "Church Registration Number must be in format '10802XXXX'.")]
        public string? ChurchRegistrationNumber { get; set; }

        [StringLength(8, MinimumLength = 8, ErrorMessage = "Temporary ID must be 8 characters long.")]
        [RegularExpression(@"^TMP-\d{4}$", ErrorMessage = "Temporary ID must be in format 'TMP-XXXX'.")]
        public string? TemporaryID { get; set; }

        [Required]
        public FamilyStatus Status { get; set; } = FamilyStatus.Active;

        [StringLength(150, ErrorMessage = "Migrated to location cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }

        [StringLength(50, ErrorMessage = "House number cannot exceed 50 characters.")]
        public string? HouseNumber { get; set; }

        [StringLength(100, ErrorMessage = "Street name cannot exceed 100 characters.")]
        public string? StreetName { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }

        [StringLength(20, ErrorMessage = "Post code cannot exceed 20 characters.")]
        public string? PostCode { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        public bool GiftAid { get; set; }

        [StringLength(150, ErrorMessage = "Parish in India cannot exceed 150 characters.")]
        public string? ParishIndia { get; set; }

        [StringLength(150, ErrorMessage = "Eparchy in India cannot exceed 150 characters.")]
        public string? EparchyIndia { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }
    }
}