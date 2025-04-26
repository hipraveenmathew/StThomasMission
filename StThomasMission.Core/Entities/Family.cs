using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class Family
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Family name is required.")]
        [StringLength(150, ErrorMessage = "Family name cannot exceed 150 characters.")]
        public string FamilyName { get; set; } = string.Empty;

        [Required]
        public int WardId { get; set; } // Foreign key to Ward
        public Ward Ward { get; set; } = null!; // Navigation property

        public bool IsRegistered { get; set; }

        [RegularExpression(@"^10802\d{4}$", ErrorMessage = "Church registration number must be in the format '10802XXXX'.")]
        public string? ChurchRegistrationNumber { get; set; }

        [RegularExpression(@"^TMP-\d{4}$", ErrorMessage = "Temporary ID must be in the format 'TMP-XXXX'.")]
        public string? TemporaryID { get; set; }

        [Required]
        public FamilyStatus Status { get; set; } = FamilyStatus.Active;

        [StringLength(150, ErrorMessage = "Migration target cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!; // Optimistic concurrency

        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
        public ICollection<MigrationLog> MigrationLogs { get; set; } = new List<MigrationLog>();

        // Suggested index: WardId, Status
    }
}