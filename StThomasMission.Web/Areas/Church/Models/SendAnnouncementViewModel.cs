using Microsoft.AspNetCore.Mvc.Rendering;
using StThomasMission.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Church.Models
{
    public class SendAnnouncementViewModel
    {
        [Required(ErrorMessage = "Please select a ward.")]
        [Display(Name = "Target Ward")]
        public int WardId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a communication channel.")]
        [Display(Name = "Send Via")]
        public CommunicationChannel Channel { get; set; }

        // For populating the dropdown list
        public SelectList? AvailableWards { get; set; }
    }
}