using StThomasMission.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class UpdateMassTimingRequest
    {
        [Required]
        [StringLength(20)]
        public string Day { get; set; } = string.Empty;

        [Required]
        public TimeSpan Time { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public MassType Type { get; set; }

        [Required]
        public DateTime WeekStartDate { get; set; }
    }
}