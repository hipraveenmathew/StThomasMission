using StThomasMission.Core.Enums;
using System;

namespace StThomasMission.Core.DTOs
{
    public class GroupActivityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int Points { get; set; }
        public ActivityStatus Status { get; set; }
    }
}