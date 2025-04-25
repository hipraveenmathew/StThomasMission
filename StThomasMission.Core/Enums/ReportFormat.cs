using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum ReportFormat
    {
        [Display(Name = "PDF")]
        Pdf,
        [Display(Name = "Excel")]
        Excel
    }
}
