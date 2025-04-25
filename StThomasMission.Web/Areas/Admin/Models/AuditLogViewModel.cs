namespace StThomasMission.Web.Areas.Admin.Models
{
    public class AuditLogViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string Details { get; set; }
        public string Username { get; set; }
    }
}
