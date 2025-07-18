using System;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Admin.Models
{
    public class AnnouncementFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Posted Date")]
        [DataType(DataType.Date)]
        public DateTime PostedDate { get; set; } = DateTime.Today;

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; } = true;
    }
}