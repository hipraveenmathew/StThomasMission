using StThomasMission.Core.DTOs;
using StThomasMission.Core.Interfaces;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class AuditLogIndexViewModel
    {
        public AuditLogFilterViewModel Filter { get; set; } = new();
        public IPaginatedList<AuditLogDto> Logs { get; set; } = null!;
        public string? CurrentSort { get; set; } // <-- Add this property
    }
}