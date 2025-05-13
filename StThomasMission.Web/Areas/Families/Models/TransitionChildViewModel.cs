using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Families.Models
{
    public class TransitionChildViewModel
    {
        public int FamilyMemberId { get; set; }
        public string ChildName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string NewFamilyName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int WardId { get; set; }

        public bool IsRegistered { get; set; }
    }
}
