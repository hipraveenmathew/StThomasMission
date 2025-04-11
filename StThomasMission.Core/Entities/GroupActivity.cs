namespace StThomasMission.Core.Entities
{
    public class GroupActivity
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ActivityName { get; set; } = string.Empty;
        public int Points { get; set; }
        public DateTime Date { get; set; }
    }
}