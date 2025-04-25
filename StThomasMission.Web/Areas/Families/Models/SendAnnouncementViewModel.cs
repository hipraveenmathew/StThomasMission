namespace StThomasMission.Web.Areas.Families.Models
{
    public class SendAnnouncementViewModel
    {
        public string Message { get; set; }
        public string? Ward { get; set; }
        public List<string> CommunicationMethods { get; set; } = new List<string>();
    }
}