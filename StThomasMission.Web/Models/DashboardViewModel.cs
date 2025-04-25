using StThomasMission.Core.Entities;

namespace StThomasMission.Web.Models
{
    public class DashboardViewModel
    {
        public List<MessageLog> RecentAnnouncements { get; set; } = new();
        public List<GroupActivity> UpcomingEvents { get; set; } = new();
        public List<Student> StudentProgress { get; set; } = new();
    }
}
