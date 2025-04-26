using StThomasMission.Web.Models;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class AuditLogIndexViewModel
    {
        public AuditLogFilterViewModel Filter { get; set; } = new AuditLogFilterViewModel();
        public PaginatedList<AuditLogViewModel> Logs { get; set; }
    }
}