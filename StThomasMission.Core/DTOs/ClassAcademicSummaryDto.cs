namespace StThomasMission.Core.DTOs
{
    public class ClassAcademicSummaryDto
    {
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? Remarks { get; set; }
    }
}