using System.ComponentModel.DataAnnotations;
using StThomasMission.Core.Entities;

namespace StThomasMission.Core.Entities
{
    public class TeacherAssignment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } = null!; // The Teacher

        [Required]
        public int GradeId { get; set; } // Foreign key to Grade
        public Grade Grade { get; set; } = null!; // The Grade they teach

        // Optional: If a teacher manages a specific group within a grade
        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        [Required]
        public int AcademicYear { get; set; } // The year this assignment is valid for
    }
}