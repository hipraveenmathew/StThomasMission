namespace StThomasMission.Core.Entities
{
    public class FamilyMember
    {
        public int Id { get; set; }
        public int FamilyId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Relation { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }

        public Family Family { get; set; } = null!;
        public Student? StudentProfile { get; set; }
    }
}