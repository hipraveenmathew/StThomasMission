using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Group name is required.")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty; // e.g., St. Peter

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!; // Optimistic concurrency

        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<GroupActivity> GroupActivities { get; set; } = new List<GroupActivity>();

        // Suggested index: Name
    }
}