using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(450)] // Align with Identity UserId length
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action is required.")]
        [MaxLength(150)]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entity name is required.")]
        [MaxLength(150)]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public int EntityId { get; set; }

        [Required(ErrorMessage = "Details are required.")]
        [MaxLength(1000)]
        public string Details { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
