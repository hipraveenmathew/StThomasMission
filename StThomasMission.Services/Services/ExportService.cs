using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class ExportService : IExportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> ExportFamiliesAsync(ReportFormat format)
        {
            var families = await _unitOfWork.Families.GetAllAsync();

            if (format == ReportFormat.Pdf)
            {
                return await GenerateFamiliesPdfAsync(families);
            }
            else // Excel
            {
                return await GenerateFamiliesExcelAsync(families);
            }
        }

        public async Task<byte[]> ExportStudentsAsync(ReportFormat format)
        {
            var students = await _unitOfWork.Students.GetAllAsync();

            if (format == ReportFormat.Pdf)
            {
                return await GenerateStudentsPdfAsync(students);
            }
            else // Excel
            {
                return await GenerateStudentsExcelAsync(students);
            }
        }

        public async Task<byte[]> ExportAttendanceAsync(DateTime? startDate, DateTime? endDate, ReportFormat format)
        {
            var attendance = await _unitOfWork.Attendances.GetAsync(a =>
                (!startDate.HasValue || a.Date >= startDate) &&
                (!endDate.HasValue || a.Date <= endDate));

            if (format == ReportFormat.Pdf)
            {
                return await GenerateAttendancePdfAsync(attendance);
            }
            else // Excel
            {
                return await GenerateAttendanceExcelAsync(attendance);
            }
        }

        private async Task<byte[]> GenerateFamiliesExcelAsync(IEnumerable<Family> families)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Families");

            worksheet.Cells[1, 1].Value = "Family Name";
            worksheet.Cells[1, 2].Value = "Ward ID";
            worksheet.Cells[1, 3].Value = "Is Registered";
            worksheet.Cells[1, 4].Value = "Church Registration Number";
            worksheet.Cells[1, 5].Value = "Temporary ID";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Created Date";

            int row = 2;
            foreach (var f in families)
            {
                worksheet.Cells[row, 1].Value = f.FamilyName;
                worksheet.Cells[row, 2].Value = f.WardId;
                worksheet.Cells[row, 3].Value = f.IsRegistered;
                worksheet.Cells[row, 4].Value = f.ChurchRegistrationNumber;
                worksheet.Cells[row, 5].Value = f.TemporaryID;
                worksheet.Cells[row, 6].Value = f.Status.ToString();
                worksheet.Cells[row, 7].Value = f.CreatedDate.ToString("yyyy-MM-dd");
                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task<byte[]> GenerateStudentsExcelAsync(IEnumerable<Student> students)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");

            worksheet.Cells[1, 1].Value = "Full Name";
            worksheet.Cells[1, 2].Value = "Grade";
            worksheet.Cells[1, 3].Value = "Academic Year";
            worksheet.Cells[1, 4].Value = "Group ID";
            worksheet.Cells[1, 5].Value = "Status";

            int row = 2;
            foreach (var s in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(s.FamilyMemberId);
                worksheet.Cells[row, 1].Value = familyMember?.FullName ?? "Unknown";
                worksheet.Cells[row, 2].Value = s.Grade;
                worksheet.Cells[row, 3].Value = s.AcademicYear;
                worksheet.Cells[row, 4].Value = s.GroupId;
                worksheet.Cells[row, 5].Value = s.Status.ToString();
                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task<byte[]> GenerateAttendanceExcelAsync(IEnumerable<Attendance> attendance)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Attendance");

            worksheet.Cells[1, 1].Value = "Student ID";
            worksheet.Cells[1, 2].Value = "Date";
            worksheet.Cells[1, 3].Value = "Status";
            worksheet.Cells[1, 4].Value = "Description";

            int row = 2;
            foreach (var a in attendance)
            {
                worksheet.Cells[row, 1].Value = a.StudentId;
                worksheet.Cells[row, 2].Value = a.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 3].Value = a.Status.ToString();
                worksheet.Cells[row, 4].Value = a.Description;
                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task<byte[]> GenerateFamiliesPdfAsync(IEnumerable<Family> families)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Content().Column(column =>
                        {
                            column.Item().Text("Families Export").FontSize(16).Bold();
                            column.Item().Text($"Total Families: {families.Count()}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            foreach (var f in families)
                            {
                                column.Item().PaddingVertical(5).Text(text =>
                                {
                                    text.Span("Family Name: ").Bold();
                                    text.Span(f.FamilyName);
                                    text.Line($"Ward ID: {f.WardId}");
                                    text.Line($"Is Registered: {f.IsRegistered}");
                                    text.Line($"Church Registration Number: {f.ChurchRegistrationNumber ?? "N/A"}");
                                    text.Line($"Temporary ID: {f.TemporaryID ?? "N/A"}");
                                    text.Line($"Status: {f.Status}");
                                    text.Line($"Created Date: {f.CreatedDate:yyyy-MM-dd}");
                                });
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        private async Task<byte[]> GenerateStudentsPdfAsync(IEnumerable<Student> students)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Content().Column(column =>
                        {
                            column.Item().Text("Students Export").FontSize(16).Bold();
                            column.Item().Text($"Total Students: {students.Count()}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            foreach (var s in students)
                            {
                                var familyMemberTask = _unitOfWork.FamilyMembers.GetByIdAsync(s.FamilyMemberId);
                                familyMemberTask.Wait();
                                var familyMember = familyMemberTask.Result;

                                column.Item().PaddingVertical(5).Text(text =>
                                {
                                    text.Span("Name: ").Bold();
                                    text.Span(familyMember?.FullName ?? "Unknown");
                                    text.Line($"Grade: {s.Grade}");
                                    text.Line($"Academic Year: {s.AcademicYear}");
                                    text.Line($"Group ID: {s.GroupId}");
                                    text.Line($"Status: {s.Status}");
                                });
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        private async Task<byte[]> GenerateAttendancePdfAsync(IEnumerable<Attendance> attendance)
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Content().Column(column =>
                        {
                            column.Item().Text("Attendance Export").FontSize(16).Bold();
                            column.Item().Text($"Total Records: {attendance.Count()}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            foreach (var a in attendance)
                            {
                                column.Item().PaddingVertical(5).Text(text =>
                                {
                                    text.Span("Student ID: ").Bold();
                                    text.Span(a.StudentId.ToString());
                                    text.Line($"Date: {a.Date:yyyy-MM-dd}");
                                    text.Line($"Status: {a.Status}");
                                    text.Line($"Description: {a.Description}");
                                });
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }
    }
}