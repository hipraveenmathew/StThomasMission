namespace StThomasMission.Core.Entities
{
    public class GroupActivity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int Points { get; set; } // Add this
    }
}