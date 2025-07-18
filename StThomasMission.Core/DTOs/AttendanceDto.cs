using StThomasMission.Core.Enums;
using System;

namespace StThomasMission.Core.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
    }
}