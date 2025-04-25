using StThomasMission.Core.Entities;

namespace StThomasMission.Web.Areas.Parents.Models
{
    public class ParentPortalViewModel
    {
        public Family Family { get; set; }
        public List<Student> Students { get; set; } = new();
    }
}
