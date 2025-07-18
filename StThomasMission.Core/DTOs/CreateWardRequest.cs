using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class CreateWardRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}