using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum ImportType
    {
        [Display(Name = "Excel")]
        Excel,
        [Display(Name = "CSV")]
        CSV
    }
}