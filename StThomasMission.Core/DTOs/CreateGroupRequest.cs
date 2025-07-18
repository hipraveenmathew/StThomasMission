using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class CreateGroupRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}