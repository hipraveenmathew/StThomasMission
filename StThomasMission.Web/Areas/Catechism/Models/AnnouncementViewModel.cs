namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class AbsenteeNotificationViewModel
    {
        public int Grade { get; set; }
    }

    public class AnnouncementViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string? Ward { get; set; }
    }
}