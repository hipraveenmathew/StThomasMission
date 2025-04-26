using StThomasMission.Core.Entities;
using System.Collections.Generic;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class StudentDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Grade { get; set; }
        public int GroupId { get; set; }
        public string StudentOrganisation { get; set; }
        public string Status { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<Assessment> Assessments { get; set; }
    }
}