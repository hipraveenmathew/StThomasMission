using StThomasMission.Web.Areas.Catechism.Models;
using System.Collections.Generic;

namespace StThomasMission.Web.Models
{
    public class EventsViewModel
    {
        public List<AnnouncementViewModel> Announcements { get; set; } = new List<AnnouncementViewModel>();
    }
}