using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class UserIndexViewModel
    {
        public IPaginatedList<UserDto> PagedUsers { get; set; } = null!;
        public string? SearchTerm { get; set; }
    }
}