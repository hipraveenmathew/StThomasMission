using StThomasMission.Core.Enums;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyFilterViewModel
    {
        public string? SearchString { get; set; }
        public string? Ward { get; set; }
        public FamilyStatus? Status { get; set; }
    }
}