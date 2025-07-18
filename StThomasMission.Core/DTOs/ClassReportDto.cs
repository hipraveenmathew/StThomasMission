using System.Collections.Generic;

namespace StThomasMission.Core.DTOs.Reporting
{
    public class ClassReportDto
    {
        public string GradeName { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public int TotalStudents { get; set; }
        public List<ClassReportStudentSummary> Students { get; set; } = new List<ClassReportStudentSummary>();
    }
}