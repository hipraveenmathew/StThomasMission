using StThomasMission.Core.DTOs;
using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // Stats
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int TotalFamilies { get; set; }
        public int RegisteredFamilies { get; set; }
        public int TotalWards { get; set; }
        public int TotalGroups { get; set; }

        // "Top 5" Lists
        public List<AnnouncementSummaryDto> RecentAnnouncements { get; set; } = new();
        public List<GroupActivityDto> UpcomingEvents { get; set; } = new();
    }
}