using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class FeeReminderViewModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public string FeeDetails { get; set; }
    }
}
