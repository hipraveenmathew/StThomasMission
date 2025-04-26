using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class AbsenteeNotificationViewModel
    {
        [Required]
        public string Grade { get; set; }

        [Required(ErrorMessage = "Please select at least one communication method.")]
        public List<string> CommunicationMethods { get; set; } = new List<string>();

        public string? SuccessMessage { get; set; }
    }
}