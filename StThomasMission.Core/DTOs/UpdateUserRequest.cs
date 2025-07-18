using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class UpdateUserRequest
    {
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public int WardId { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }
    }
}