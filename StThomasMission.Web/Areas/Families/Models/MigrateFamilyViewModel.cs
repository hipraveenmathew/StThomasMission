using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Web.Areas.Church.Models
{
    public class MigrateFamilyViewModel
    {
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Migrated To Location")]
        public string MigratedTo { get; set; } = string.Empty;
    }
}