namespace StThomasMission.Web.Areas.Families.Models
{
    public class FamilyViewModel
    {
        public string FamilyName { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public string? ChurchRegistrationNumber { get; set; }
        public string? TemporaryId { get; set; }
    }

    public class FamilyMemberViewModel
    {
        public int FamilyId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Relation { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }
    }

    public class StudentEnrollmentViewModel
    {
        public int FamilyMemberId { get; set; }
        public string Grade { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string Group { get; set; } = string.Empty;
        public string? StudentOrganisation { get; set; }
    }
}