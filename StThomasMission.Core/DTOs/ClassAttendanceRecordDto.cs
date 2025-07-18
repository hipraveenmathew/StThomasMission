using StThomasMission.Core.Enums;
using System;

namespace StThomasMission.Core.DTOs
{
    public class ClassAttendanceRecordDto
    {
        public int AttendanceId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public int StudentId { get; set; }
        public string StudentFullName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}