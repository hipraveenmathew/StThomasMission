using StThomasMission.Core.DTOs;
using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class AnnouncementIndexViewModel
    {
        public List<AnnouncementSummaryDto> Announcements { get; set; } = new List<AnnouncementSummaryDto>();
    }
}