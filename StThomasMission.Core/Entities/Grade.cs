namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Represents a catechism grade level (e.g., Year 1, Year 12).
    /// </summary>
    public class Grade
    {
        public int Id { get; set; }

        // e.g., "Year 1", "Year 12", "Kindergarten"
        public string Name { get; set; } = string.Empty;

        // The sequence for promotion, e.g., 1, 2, ..., 12
        public int Order { get; set; }

        // --- Navigation Properties ---
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
    }
}