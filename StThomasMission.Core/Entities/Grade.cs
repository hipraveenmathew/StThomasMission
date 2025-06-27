namespace StThomasMission.Core.Entities
{
    public class Grade
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Year 1", "Year 12", "Kindergarten"
        public int Order { get; set; } // The sequence for promotion, e.g., 1, 2, 3... 12

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}