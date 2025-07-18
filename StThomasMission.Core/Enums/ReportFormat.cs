using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    /// <summary>
    /// Defines the available formats for exporting reports.
    /// </summary>
    public enum ReportFormat
    {
        [Display(Name = "PDF")]
        PDF,

        [Display(Name = "Excel")]
        Excel
    }
}