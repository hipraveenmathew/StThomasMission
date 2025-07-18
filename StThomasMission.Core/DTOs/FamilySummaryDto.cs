using StThomasMission.Core.Enums;

namespace StThomasMission.Core.DTOs
{
    public class FamilySummaryDto
    {
        public int Id { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public string? ChurchRegistrationNumber { get; set; }
        public string? TemporaryID { get; set; }
        public FamilyStatus Status { get; set; }
    }
}