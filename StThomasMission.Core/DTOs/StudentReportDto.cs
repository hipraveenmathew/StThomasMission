using StThomasMission.Core.Enums;
using System.Collections.Generic;

namespace StThomasMission.Core.DTOs.Reporting
{
    public class StudentReportDto
    {
        public string FullName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public StudentStatus Status { get; set; }
        public List<AssessmentDto> Assessments { get; set; } = new List<AssessmentDto>();
        public List<AttendanceDto> Attendances { get; set; } = new List<AttendanceDto>();
    }
}