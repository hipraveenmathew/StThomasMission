namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class AssessmentViewModel
    {
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Marks { get; set; }
        public int TotalMarks { get; set; }
        public bool IsMajor { get; set; }
    }
}