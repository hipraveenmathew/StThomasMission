using StThomasMission.Core.Enums;
using System.Collections.Generic;

namespace StThomasMission.Core.DTOs
{
    public class ImportFamilyData
    {
        public string FamilyName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public int WardId { get; set; }
        public string? ChurchRegistrationNumber { get; set; }
        public List<ImportMemberData> Members { get; set; } = new List<ImportMemberData>();
    }

    public class ImportMemberData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public FamilyMemberRole Role { get; set; }
    }
}