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
        public int WardId { get; set; }
        public Ward Ward { get; set; } = null!;

        public bool IsRegistered { get; set; }

        [RegularExpression(@"^108020\d{3}$", ErrorMessage = "Church registration number must be in the format '108020XXX'.")]
        public string? ChurchRegistrationNumber { get; set; }

        [RegularExpression(@"^TMP-\d{4}$", ErrorMessage = "Temporary ID must be in the format 'TMP-XXXX'.")]
        public string? TemporaryID { get; set; }

        [Required]
        public FamilyStatus Status { get; set; } = FamilyStatus.Active;

        [StringLength(150, ErrorMessage = "Migration target cannot exceed 150 characters.")]
        public string? MigratedTo { get; set; }

        [StringLength(50, ErrorMessage = "House number cannot exceed 50 characters.")]
        public string? HouseNumber { get; set; }

        [StringLength(100, ErrorMessage = "Street name cannot exceed 100 characters.")]
        public string? StreetName { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }

        [StringLength(20, ErrorMessage = "Post code cannot exceed 20 characters.")]
        public string? PostCode { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string? Email { get; set; }

        public bool GiftAid { get; set; }

        // Moved from FamilyMember to Family (common to all members)
        [StringLength(150, ErrorMessage = "Parish in India cannot exceed 150 characters.")]
        public string? ParishIndia { get; set; }

        [StringLength(150, ErrorMessage = "Eparchy in India cannot exceed 150 characters.")]
        public string? EparchyIndia { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public int? PreviousFamilyId { get; set; }
        public Family? PreviousFamily { get; set; }

        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
        public ICollection<MigrationLog> MigrationLogs { get; set; } = new List<MigrationLog>();
    }
}