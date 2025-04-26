using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class ClassAttendanceViewModel
    {
        [Required]
        public string Grade { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public List<StudentAttendanceViewModel> Students { get; set; } = new List<StudentAttendanceViewModel>();
    }

    public class StudentAttendanceViewModel
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public bool IsPresent { get; set; }
    }
}