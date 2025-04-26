using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class MigrationLog
    {
        public int Id { get; set; }

        [Required]
        public int FamilyId { get; set; }
        public Family Family { get; set; } = null!;

        [Required]
        [StringLength(150, ErrorMessage = "MigratedTo cannot exceed 150 characters.")]
        public string MigratedTo { get; set; } = string.Empty;

        [Required]
        public DateTime MigrationDate { get; set; }

        [Required]
        [StringLength(150)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(150)]
        public string? UpdatedBy { get; set; } // Added
    }
}