using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class AuditLogFilterViewModel
    {
        [Display(Name = "User ID")]
        public string? UserId { get; set; }

        [Display(Name = "Entity Name")]
        public string? EntityName { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}