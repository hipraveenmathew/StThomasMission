using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents a student group for activities (e.g., St. Peter's Group).
    /// </summary>
    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Group name is required.")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

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

        // --- Navigation Properties ---
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<GroupActivity> GroupActivities { get; set; } = new List<GroupActivity>();
    }
}