using StThomasMission.Core.Entities;
using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Parents.Models
{
    public class ParentPortalViewModel
    {
        public Family Family { get; set; }
        public List<Student> Students { get; set; }
        public Dictionary<int, List<Attendance>> StudentAttendances { get; set; } // Key: StudentId
        public Dictionary<int, List<Assessment>> StudentAssessments { get; set; } // Key: StudentId
    }
}