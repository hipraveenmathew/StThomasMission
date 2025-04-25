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
        Inactive, // Added for families temporarily inactive
        [Display(Name = "Deleted")]
        Deleted  // Added for soft-deleted families
    }
}
