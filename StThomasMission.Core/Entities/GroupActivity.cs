using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class GroupActivity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [StringLength(150, ErrorMessage = "Activity name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Group is required.")]
        [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters.")]
        public string Group { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        public ActivityStatus Status { get; set; } = ActivityStatus.Active;

        [Range(0, int.MaxValue, ErrorMessage = "Points cannot be negative.")]
        public int Points { get; set; }
    }
}
