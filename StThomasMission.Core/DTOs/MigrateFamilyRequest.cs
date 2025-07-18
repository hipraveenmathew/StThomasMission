using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class MigrateFamilyRequest
    {
        [Required]
        [StringLength(150)]
        public string MigratedTo { get; set; } = string.Empty;
    }
}