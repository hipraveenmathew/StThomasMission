using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class LogMigrationRequest
    {
        [Required]
        public int FamilyId { get; set; }

        [Required]
        [StringLength(150)]
        public string MigratedTo { get; set; } = string.Empty;

        [Required]
        public DateTime MigrationDate { get; set; }
    }
}