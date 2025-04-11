namespace StThomasMission.Core.Entities
{
    public class Family
    {
        public int Id { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public string? ChurchRegistrationNumber { get; set; } // e.g., "108020001"
        public string? TemporaryID { get; set; } // e.g., "TMP-0001"
        public string? Status { get; set; } // Active, Migrated
        public string? MigratedTo { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<FamilyMember> Members { get; set; } = new List<FamilyMember>();
    }
}