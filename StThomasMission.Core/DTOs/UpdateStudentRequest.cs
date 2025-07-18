using StThomasMission.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class UpdateStudentRequest
    {
        [Required]
        public int GradeId { get; set; }
        public int? GroupId { get; set; }

        [StringLength(150)]
        public string? StudentOrganisation { get; set; }

        [Required]
        public StudentStatus Status { get; set; }

        [StringLength(150)]
        public string? MigratedTo { get; set; }
    }
}