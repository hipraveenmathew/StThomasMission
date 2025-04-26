using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyDetailsViewModel : FamilyViewModel
    {
        public List<FamilyMemberViewModel> Members { get; set; } = new List<FamilyMemberViewModel>();
    }
}