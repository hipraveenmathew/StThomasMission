using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action is required.")]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entity name is required.")]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public int EntityId { get; set; }

        [Required(ErrorMessage = "Details are required.")]
        public string Details { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }
    }
}