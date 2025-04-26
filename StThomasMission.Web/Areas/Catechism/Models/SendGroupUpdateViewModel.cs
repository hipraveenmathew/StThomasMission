using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class SendGroupUpdateViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid group ID.")]
        public int GroupId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Update message cannot exceed 1000 characters.")]
        public string UpdateMessage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select at least one communication method.")]
        public List<string> CommunicationMethods { get; set; } = new List<string>();

        public string? SuccessMessage { get; set; }
    }
}