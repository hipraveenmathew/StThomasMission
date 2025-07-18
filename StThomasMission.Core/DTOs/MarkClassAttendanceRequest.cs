using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StThomasMission.Core.DTOs
{
    public class MarkClassAttendanceRequest
    {
        [Required]
        public int GradeId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [StringLength(250)]
        public string Description { get; set; } = "Catechism Class";

        [Required]
        public List<StudentAttendanceRecord> Records { get; set; } = new List<StudentAttendanceRecord>();
    }
    public class StudentAttendanceRecord
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
