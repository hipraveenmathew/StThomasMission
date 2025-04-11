namespace StThomasMission.Core.Entities
{
    public class Attendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = "Catechism Class";
        public bool IsPresent { get; set; }

        public Student Student { get; set; } = null!;
    }
}