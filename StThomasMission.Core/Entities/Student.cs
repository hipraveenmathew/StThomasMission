namespace StThomasMission.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public int FamilyMemberId { get; set; }
        public int AcademicYear { get; set; }
        public string Grade { get; set; } = string.Empty; // e.g., Year 1
        public string? Group { get; set; }
        public string? StudentOrganisation { get; set; }
        public string Status { get; set; } = "Active"; // Active, Graduated, Migrated
        public string? MigratedTo { get; set; }

        public FamilyMember FamilyMember { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}