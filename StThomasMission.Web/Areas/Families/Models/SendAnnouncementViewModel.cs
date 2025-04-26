using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class SendAnnouncementViewModel
    {
        [Required]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid ward.")]
        public int Ward { get; set; }

        [Required(ErrorMessage = "Please select at least one communication method.")]
        public List<string> CommunicationMethods { get; set; } = new List<string>();

        public string? SuccessMessage { get; set; }
    }
}