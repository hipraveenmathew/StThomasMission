using StThomasMission.Core.Entities;
using StThomasMission.Web.Models;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyIndexViewModel
    {
        public FamilyFilterViewModel Filter { get; set; } = new FamilyFilterViewModel();
        public PaginatedList<Family> Families { get; set; }
    }
}