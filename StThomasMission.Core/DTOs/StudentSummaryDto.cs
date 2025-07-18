using StThomasMission.Core.Enums;

namespace StThomasMission.Core.DTOs
{
    public class StudentSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string? GroupName { get; set; }
        public StudentStatus Status { get; set; }
    }
}