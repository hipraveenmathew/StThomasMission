using System;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class AuditLogFilterViewModel
    {
        public string SearchString { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}