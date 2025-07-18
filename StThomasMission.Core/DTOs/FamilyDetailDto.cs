using StThomasMission.Core.Enums;
using System.Collections.Generic;

namespace StThomasMission.Core.DTOs
{
    public class FamilyDetailDto
    {
        public int Id { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public bool IsRegistered { get; set; }
        public int WardId { get; set; }
        public string? ChurchRegistrationNumber { get; set; }
        public string? TemporaryID { get; set; }
        public FamilyStatus Status { get; set; }
        public string? HouseNumber { get; set; }
        public string? StreetName { get; set; }
        public string? City { get; set; }
        public string? PostCode { get; set; }
        public string? Email { get; set; }
        public bool GiftAid { get; set; }
        public List<FamilyMemberDto> Members { get; set; } = new List<FamilyMemberDto>();
    }
}