using StThomasMission.Core.Enums;
using System;

namespace StThomasMission.Core.DTOs
{
    public class AssessmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public AssessmentType Type { get; set; }
        public double Marks { get; set; }
        public double TotalMarks { get; set; }
        public double Percentage { get; set; }
        public string? Remarks { get; set; }
    }
}