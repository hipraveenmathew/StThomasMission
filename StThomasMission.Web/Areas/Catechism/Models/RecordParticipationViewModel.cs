using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Catechism.Models
{
    public class RecordParticipationViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid student ID.")]
        public int StudentId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid group activity ID.")]
        public int GroupActivityId { get; set; }

        public string? SuccessMessage { get; set; }
    }
}