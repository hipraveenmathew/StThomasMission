using StThomasMission.Core.DTOs;
using System.Collections.Generic;

namespace StThomasMission.Web.Models
{
    public class HomeViewModel
    {
        // This property should be a list of the WEB-specific view model
        public List<AnnouncementViewModel> Announcements { get; set; } = new List<AnnouncementViewModel>();

        public List<MassTimingDto> MassTimings { get; set; } = new List<MassTimingDto>();
    }
}