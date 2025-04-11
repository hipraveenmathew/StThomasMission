namespace StThomasMission.Core.Entities
{
    public class Assessment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Marks { get; set; }
        public int TotalMarks { get; set; }
        public DateTime Date { get; set; }
        public bool IsMajor { get; set; }

        public Student Student { get; set; } = null!;
    }
}