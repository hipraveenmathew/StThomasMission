namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class AbsenteeNotificationViewModel
    {
        public string Grade { get; set; }
        public List<string> CommunicationMethods { get; set; } = new();
    }

    public class AnnouncementViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string? Ward { get; set; }
    }
}