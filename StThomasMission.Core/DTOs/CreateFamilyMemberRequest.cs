using StThomasMission.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class CreateFamilyMemberRequest
    {
        [Required]
        public int FamilyId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public FamilyMemberRole Relation { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Contact { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? BaptismalName { get; set; }
    }
}