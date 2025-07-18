using StThomasMission.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class UpdateAssessmentRequest
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 1000)]
        public double Marks { get; set; }

        [Range(1, 1000)]
        public double TotalMarks { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AssessmentType Type { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}