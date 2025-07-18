using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class UserRoleIndexViewModel
    {
        public IPaginatedList<UserDto> PagedUsers { get; set; } = null!;
        public SelectList RolesForDropdown { get; set; } = null!;
        public string? SearchTerm { get; set; }
    }
}