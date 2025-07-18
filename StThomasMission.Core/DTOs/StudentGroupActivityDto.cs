using System;

namespace StThomasMission.Core.DTOs
{
    public class StudentGroupActivityDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public int GroupActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public DateTime ParticipationDate { get; set; }
        public int PointsEarned { get; set; }
        public string? Remarks { get; set; }
    }
}