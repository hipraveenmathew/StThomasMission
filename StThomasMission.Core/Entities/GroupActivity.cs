using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Enums;

namespace StThomasMission.Core.Entities
{
    public class GroupActivity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Group is required.")]
        [MaxLength(100)]
        public string Group { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        public ActivityStatus Status { get; set; } = ActivityStatus.Active;

        [Range(0, int.MaxValue, ErrorMessage = "Points cannot be negative.")]
        public int Points { get; set; }
    }
}
