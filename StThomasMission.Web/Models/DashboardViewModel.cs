using StThomasMission.Core.Entities;
using System.Collections.Generic;

namespace StThomasMission.Web.Models
{
    public class DashboardViewModel
    {
        public PaginatedList<MessageLog> RecentAnnouncements { get; set; }
        public PaginatedList<GroupActivity> UpcomingEvents { get; set; }
        public PaginatedList<Student> StudentProgress { get; set; }

        public DashboardViewModel()
        {
            RecentAnnouncements = new PaginatedList<MessageLog>(new List<MessageLog>(), 0, 1, 5);
            UpcomingEvents = new PaginatedList<GroupActivity>(new List<GroupActivity>(), 0, 1, 5);
            StudentProgress = new PaginatedList<Student>(new List<Student>(), 0, 1, 5);
        }
    }
}