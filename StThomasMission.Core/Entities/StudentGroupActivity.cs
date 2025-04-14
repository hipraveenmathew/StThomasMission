namespace StThomasMission.Core.Entities
{
    public class StudentGroupActivity
    {
        public int StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public int GroupActivityId { get; set; }
        public GroupActivity GroupActivity { get; set; } = null!;

        public DateTime ParticipationDate { get; set; }
    }
}