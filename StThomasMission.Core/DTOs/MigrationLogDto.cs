using System;

namespace StThomasMission.Core.DTOs
{
    public class MigrationLogDto
    {
        public int Id { get; set; }
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string? ChurchRegistrationNumber { get; set; }
        public string MigratedTo { get; set; } = string.Empty;
        public DateTime MigrationDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}