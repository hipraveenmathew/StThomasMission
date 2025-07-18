using StThomasMission.Core.DTOs;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Reporting; // Add this using
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class ExportService : IExportService
    {
        private readonly IUnitOfWork _unitOfWork;

        // The service no longer depends on the generators via DI
        public ExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> ExportStudentsByGradeAsync(int gradeId, ReportFormat format)
        {
            var students = await _unitOfWork.Students.GetByGradeIdAsync(gradeId);
            string reportTitle = $"Students - {students.FirstOrDefault()?.GradeName ?? "Unknown Grade"}";

            if (format == ReportFormat.Excel)
            {
                var generator = new ExcelReportGenerator<StudentSummaryDto>();
                return await generator.GenerateReportAsync(format, students, reportTitle);
            }
            else // PDF
            {
                var generator = new PdfReportGenerator<StudentSummaryDto>();
                return await generator.GenerateReportAsync(format, students, reportTitle);
            }
        }

        public async Task<byte[]> ExportFamiliesByWardAsync(int wardId, ReportFormat format)
        {
            var families = await _unitOfWork.Families.GetByWardAsync(wardId);
            string reportTitle = $"Families - {families.FirstOrDefault()?.WardName ?? "Unknown Ward"}";

            if (format == ReportFormat.Excel)
            {
                var generator = new ExcelReportGenerator<FamilySummaryDto>();
                return await generator.GenerateReportAsync(format, families, reportTitle);
            }
            else // PDF
            {
                var generator = new PdfReportGenerator<FamilySummaryDto>();
                return await generator.GenerateReportAsync(format, families, reportTitle);
            }
        }
    }
}