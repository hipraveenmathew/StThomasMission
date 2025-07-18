using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents a parish ward, a geographical or administrative division within the parish.
    /// </summary>
    public class Ward
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ward name is required.")]
        [StringLength(100, ErrorMessage = "Ward name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        // --- Auditing Fields ---
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(450)] // To store ApplicationUser.Id
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // --- Navigation Properties ---
        public ICollection<Family> Families { get; set; } = new List<Family>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}