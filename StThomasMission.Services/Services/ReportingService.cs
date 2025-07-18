using StThomasMission.Core.DTOs.Reporting;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Reporting; // Add this using
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IUnitOfWork _unitOfWork;

        // The service no longer depends on the generators via DI
        public ReportingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> GenerateStudentReportAsync(int studentId, ReportFormat format)
        {
            var reportData = await _unitOfWork.Students.GetStudentReportDataAsync(studentId);
            if (reportData == null)
            {
                throw new Exceptions.NotFoundException("Student", studentId);
            }

            if (format == ReportFormat.Excel)
            {
                var generator = new ExcelReportGenerator<StudentReportDto>();
                return await generator.GenerateReportAsync(format, new[] { reportData }, $"Student Report - {reportData.FullName}");
            }
            else // PDF
            {
                var generator = new PdfReportGenerator<StudentReportDto>();
                return await generator.GenerateReportAsync(format, new[] { reportData }, $"Student Report - {reportData.FullName}");
            }
        }

        public async Task<byte[]> GenerateClassReportAsync(int gradeId, int academicYear, ReportFormat format)
        {
            var reportData = await _unitOfWork.Students.GetClassReportDataAsync(gradeId, academicYear);
            if (reportData == null)
            {
                throw new Exceptions.NotFoundException($"Class Report for GradeId {gradeId} and Year {academicYear}", "Data");
            }

            if (format == ReportFormat.Excel)
            {
                var generator = new ExcelReportGenerator<ClassReportDto>();
                return await generator.GenerateReportAsync(format, new[] { reportData }, $"Class Report - {reportData.GradeName} ({reportData.AcademicYear})");
            }
            else // PDF
            {
                var generator = new PdfReportGenerator<ClassReportDto>();
                return await generator.GenerateReportAsync(format, new[] { reportData }, $"Class Report - {reportData.GradeName} ({reportData.AcademicYear})");
            }
        }
    }
}