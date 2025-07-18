namespace StThomasMission.Web.Models
{
    public class AnnouncementViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PostedDateFormatted { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }
}