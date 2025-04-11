namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class AttendanceViewModel
    {
        public int StudentId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = "Catechism Class";
        public bool IsPresent { get; set; }
    }
}