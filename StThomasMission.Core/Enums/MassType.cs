using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum MassType
    {
        [Display(Name = "Regular")]
        Regular,
        [Display(Name = "Special")]
        Special,
        [Display(Name = "Feast")]
        Feast
    }
}