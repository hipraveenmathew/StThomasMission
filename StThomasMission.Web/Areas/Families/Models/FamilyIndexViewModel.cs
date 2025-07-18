using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;
using StThomasMission.Web.Areas.Church.Models;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyIndexViewModel
    {
        public IPaginatedList<FamilySummaryDto> PagedFamilies { get; set; } = null!;
        public FamilyFilterViewModel Filter { get; set; } = new();
        public string? CurrentSort { get; set; }
        public SelectList AvailableWards { get; set; } = null!;
    }

    
}