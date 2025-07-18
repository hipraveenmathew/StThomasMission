using StThomasMission.Core.Enums;

namespace StThomasMission.Core.DTOs
{
    public class StudentDetailDto
    {
        public int Id { get; set; }
        public int FamilyMemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public string? GroupName { get; set; }
        public StudentStatus Status { get; set; }
        public string? StudentOrganisation { get; set; }
        public string? MigratedTo { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
    }
}