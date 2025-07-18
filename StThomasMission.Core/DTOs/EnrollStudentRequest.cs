using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class EnrollStudentRequest
    {
        [Required]
        public int FamilyMemberId { get; set; }

        [Required]
        public int AcademicYear { get; set; }

        [Required]
        public int GradeId { get; set; }

        public int? GroupId { get; set; }

        [StringLength(150)]
        public string? StudentOrganisation { get; set; }
    }
}