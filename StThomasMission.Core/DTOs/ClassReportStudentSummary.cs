using StThomasMission.Core.Enums;

namespace StThomasMission.Core.DTOs.Reporting
{
    public class ClassReportStudentSummary
    {
        public string StudentFullName { get; set; } = string.Empty;
        public StudentStatus Status { get; set; }
        public int TotalAbsences { get; set; }
        public double AverageMark { get; set; }
    }
}