using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(450)] // Matches ASP.NET Identity default
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Action is required.")]
        [StringLength(150)]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "Entity name is required.")]
        [StringLength(150)]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public int EntityId { get; set; }

        [Required(ErrorMessage = "Details are required.")]
        [StringLength(1000)]
        public string Details { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(45, ErrorMessage = "IP address cannot exceed 45 characters.")]
        public string? IpAddress { get; set; } // Supports IPv6 too

        [StringLength(250)]
        public string? PerformedBy { get; set; } // Optional: name or username of actor
    }

}
