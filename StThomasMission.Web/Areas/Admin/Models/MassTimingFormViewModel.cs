using StThomasMission.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class MassTimingFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Day { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public MassType Type { get; set; }

        [Required]
        [Display(Name = "Week Start Date")]
        [DataType(DataType.Date)]
        public DateTime WeekStartDate { get; set; } = DateTime.Today;
    }
}