using StThomasMission.Core.Enums;
using System;

namespace StThomasMission.Core.DTOs
{
    public class FamilyMemberDto
    {
        public int Id { get; set; }
        public int FamilyId { get; set; }
        public string? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public FamilyMemberRole Relation { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }
        public string? BaptismalName { get; set; }

        // Example of a computed property in a DTO
        public string FullName => $"{FirstName} {LastName}";
    }
}