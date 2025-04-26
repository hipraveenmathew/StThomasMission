using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class Ward
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ward name is required.")]
        [StringLength(100, ErrorMessage = "Ward name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!; // Optimistic concurrency

        public ICollection<Family> Families { get; set; } = new List<Family>();
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        // Suggested index: Name, IsDeleted
    }
}