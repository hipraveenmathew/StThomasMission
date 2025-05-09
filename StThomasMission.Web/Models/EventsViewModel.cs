using StThomasMission.Core.Entities;

namespace StThomasMission.Web.Models
{
    public class EventsViewModel
    {
        public IEnumerable<Announcement> Announcements { get; set; }
    }
}