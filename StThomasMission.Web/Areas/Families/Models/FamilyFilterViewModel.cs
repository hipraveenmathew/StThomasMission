using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyFilterViewModel
    {
        [Display(Name = "Search by Name or ID")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Ward")]
        public int? WardId { get; set; }

        [Display(Name = "Registration Status")]
        public bool? IsRegistered { get; set; }
    }
}