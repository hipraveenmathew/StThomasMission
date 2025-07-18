using System.Collections.Generic;

namespace StThomasMission.Core.DTOs
{
    // This DTO is used to pass the complex dashboard data from the service to the controller
    public class DashboardViewModelDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int TotalFamilies { get; set; }
        public int RegisteredFamilies { get; set; }
        public int TotalWards { get; set; }
        public int TotalGroups { get; set; }

        public List<AnnouncementSummaryDto> RecentAnnouncements { get; set; } = new();
        public List<GroupActivityDto> UpcomingEvents { get; set; } = new();
    }
}