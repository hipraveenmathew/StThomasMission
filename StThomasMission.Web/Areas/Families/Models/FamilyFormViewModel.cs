using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Church.Models
{
    public class FamilyFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Family Name")]
        public string FamilyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a ward.")]
        [Display(Name = "Ward")]
        public int WardId { get; set; }

        // Other properties from RegisterFamilyRequest DTO...
        public string? HouseNumber { get; set; }
        public string? StreetName { get; set; }
        public string? City { get; set; }
        public string? PostCode { get; set; }
        public string? Email { get; set; }
        public bool GiftAid { get; set; }

        public SelectList AvailableWards { get; set; } = null!;
    }
}