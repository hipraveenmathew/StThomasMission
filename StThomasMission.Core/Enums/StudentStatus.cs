using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum StudentStatus
    {
        [Display(Name = "Active")]
        Active,
        [Display(Name = "Graduated")]
        Graduated,
        [Display(Name = "Migrated")]
        Migrated,
        [Display(Name = "Inactive")]
        Inactive,
        [Display(Name = "Deleted")]
        Deleted
    }
}