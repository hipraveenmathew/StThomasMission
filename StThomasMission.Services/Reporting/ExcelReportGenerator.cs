using OfficeOpenXml;
using OfficeOpenXml.Table;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Enums;
using StThomasMission.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Reporting
{
    public class ExcelReportGenerator<T> : IReportGenerator<T> where T : class
    {
        public ExcelReportGenerator()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public Task<byte[]> GenerateReportAsync(ReportFormat format, IEnumerable<T> data, string reportTitle)
        {
            if (format != ReportFormat.Excel)
            {
                throw new System.ArgumentException("Invalid format specified for Excel generator.", nameof(format));
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(reportTitle.Length > 30 ? reportTitle.Substring(0, 30) : reportTitle);

            // Load the data from the collection of DTOs.
            // EPPlus automatically uses the property names as headers.
            worksheet.Cells["A1"].LoadFromCollection(data, true, TableStyles.Medium6);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return Task.FromResult(package.GetAsByteArray());
        }
    }
}