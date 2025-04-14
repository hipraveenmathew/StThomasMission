namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class ClassAttendanceViewModel
    {
        public string Grade { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = "Catechism Class";
        public List<StudentAttendanceViewModel> Students { get; set; } = new List<StudentAttendanceViewModel>();
    }

    public class StudentAttendanceViewModel
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
    }
}