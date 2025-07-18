namespace StThomasMission.Core.DTOs
{
    public class StudentAcademicRecordDto
    {
        public int Id { get; set; }
        public int AcademicYear { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? Remarks { get; set; }
    }
}