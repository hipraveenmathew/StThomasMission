using StThomasMission.Core.Entities;
using System.Collections.Generic;

namespace StThomasMission.Web.Models
{
    public class HomeIndexViewModel
    {
        public IEnumerable<MassTiming> MassTimings { get; set; }
        public IEnumerable<Announcement> Announcements { get; set; }
    }
}