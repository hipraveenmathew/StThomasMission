using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.DTOs
{
    public class AssignStudentsToActivityRequest
    {
        [Required]
        public List<int> StudentIds { get; set; } = new List<int>();

        [Required]
        public DateTime ParticipationDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}