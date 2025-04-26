using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class ConvertToRegisteredViewModel
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;

        [Required]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Church Registration Number must be 8 characters long.")]
        [RegularExpression(@"^10802\d{4}$", ErrorMessage = "Church Registration Number must be in format '10802XXXX'.")]
        public string ChurchRegistrationNumber { get; set; } = string.Empty;
    }
}