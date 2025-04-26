using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum FamilyStatus
    {
        [Display(Name = "Active")]
        Active,
        [Display(Name = "Migrated")]
        Migrated,
        [Display(Name = "Inactive")]
        Inactive,
        [Display(Name = "Deleted")]
        Deleted
    }
}