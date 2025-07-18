using System;

namespace StThomasMission.Core.DTOs
{
    public class AssessmentGradeViewDto
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public double Marks { get; set; }
        public double TotalMarks { get; set; }
    }
}