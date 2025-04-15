using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class GroupActivity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Group is required.")]
        public string Group { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "Active";

        [Range(0, int.MaxValue, ErrorMessage = "Points cannot be negative.")]
        public int Points { get; set; }
    }
}