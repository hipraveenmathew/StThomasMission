using StThomasMission.Core.Entities;
using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyDetailsViewModel : FamilyViewModel
    {
        public int? PreviousFamilyId { get; set; }
        public Family? PreviousFamily { get; set; }
        public List<FamilyMemberViewModel> Members { get; set; } = new List<FamilyMemberViewModel>();
    }
}