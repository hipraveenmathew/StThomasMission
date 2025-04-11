using StThomasMission.Core.Entities;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class StudentDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string? Group { get; set; }
        public string? StudentOrganisation { get; set; }
        public string Status { get; set; } = "Active";
        public List<Attendance> Attendances { get; set; } = new List<Attendance>();
        public List<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}