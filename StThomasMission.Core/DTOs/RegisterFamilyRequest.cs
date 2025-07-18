using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class RegisterFamilyRequest
    {
        [Required]
        [StringLength(150)]
        public string FamilyName { get; set; } = string.Empty;

        [Required]
        public int WardId { get; set; }

        // Optional address and contact info
        [StringLength(50)]
        public string? HouseNumber { get; set; }
        [StringLength(100)]
        public string? StreetName { get; set; }
        [StringLength(100)]
        public string? City { get; set; }
        [StringLength(20)]
        public string? PostCode { get; set; }
        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }
        public bool GiftAid { get; set; }
    }
}