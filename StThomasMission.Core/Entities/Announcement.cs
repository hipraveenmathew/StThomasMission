using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents a parish-wide announcement.
    /// </summary>
    public class Announcement
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The date the announcement is published.
        /// </summary>
        [Required]
        public DateTime PostedDate { get; set; }
        [Required]
        public string AuthorName { get; set; } = string.Empty;

        /// <summary>
        /// Controls if the announcement is currently visible to users. Managed by an admin.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        // --- Auditing Fields ---
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(450)]
        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}