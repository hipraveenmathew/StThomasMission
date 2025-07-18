using System;

namespace StThomasMission.Core.DTOs
{
    public class AnnouncementSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // Added for the view
        public DateTime PostedDate { get; set; }
        public bool IsActive { get; set; }
        public string AuthorName { get; set; } = string.Empty; // New property
    }
}