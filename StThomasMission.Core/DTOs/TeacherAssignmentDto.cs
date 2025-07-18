namespace StThomasMission.Core.DTOs
{
    public class TeacherAssignmentDto
    {
        public string UserId { get; set; } = string.Empty;
        public string TeacherFullName { get; set; } = string.Empty;
        public int GradeId { get; set; }
        public string GradeName { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public int AcademicYear { get; set; }
    }
}