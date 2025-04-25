using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
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
        }

        public async Task<byte[]> ExportFamiliesToExcelAsync()
        {
            var families = await _unitOfWork.Families.GetAllAsync();
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

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportStudentsToExcelAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Students");

            worksheet.Cells[1, 1].Value = "Full Name";
            worksheet.Cells[1, 2].Value = "Grade";
            worksheet.Cells[1, 3].Value = "Academic Year";
            worksheet.Cells[1, 4].Value = "Group";
            worksheet.Cells[1, 5].Value = "Status";

            int row = 2;
            foreach (var s in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(s.FamilyMemberId);
                worksheet.Cells[row, 1].Value = $"{familyMember.FirstName} {familyMember.LastName}";
                worksheet.Cells[row, 2].Value = s.Grade;
                worksheet.Cells[row, 3].Value = s.AcademicYear;
                worksheet.Cells[row, 4].Value = s.Group;
                worksheet.Cells[row, 5].Value = s.Status.ToString();
                row++;
            }

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportAttendanceToExcelAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var attendance = await _unitOfWork.Attendances.GetAllAsync();
            var records = attendance.Where(a =>
                (!startDate.HasValue || a.Date >= startDate) &&
                (!endDate.HasValue || a.Date <= endDate));

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Attendance");

            worksheet.Cells[1, 1].Value = "Student ID";
            worksheet.Cells[1, 2].Value = "Date";
            worksheet.Cells[1, 3].Value = "Status";
            worksheet.Cells[1, 4].Value = "Description";

            int row = 2;
            foreach (var a in records)
            {
                worksheet.Cells[row, 1].Value = a.StudentId;
                worksheet.Cells[row, 2].Value = a.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 3].Value = a.Status.ToString();
                worksheet.Cells[row, 4].Value = a.Description;
                row++;
            }

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportFamiliesToPdfAsync()
        {
            var families = await _unitOfWork.Families.GetAllAsync();
            using var memoryStream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph("Families Export"));
            document.Add(new Paragraph($"Total Families: {families.Count()}"));
            document.Add(new Paragraph("\n"));

            foreach (var f in families)
            {
                document.Add(new Paragraph($"Family Name: {f.FamilyName}"));
                document.Add(new Paragraph($"Ward ID: {f.WardId}"));
                document.Add(new Paragraph($"Is Registered: {f.IsRegistered}"));
                document.Add(new Paragraph($"Church Registration Number: {f.ChurchRegistrationNumber ?? "N/A"}"));
                document.Add(new Paragraph($"Temporary ID: {f.TemporaryID ?? "N/A"}"));
                document.Add(new Paragraph($"Status: {f.Status}"));
                document.Add(new Paragraph($"Created Date: {f.CreatedDate:yyyy-MM-dd}"));
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExportStudentsToPdfAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            using var memoryStream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph("Students Export"));
            document.Add(new Paragraph($"Total Students: {students.Count()}"));
            document.Add(new Paragraph("\n"));

            foreach (var s in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(s.FamilyMemberId);
                document.Add(new Paragraph($"Name: {familyMember.FirstName} {familyMember.LastName}"));
                document.Add(new Paragraph($"Grade: {s.Grade}"));
                document.Add(new Paragraph($"Academic Year: {s.AcademicYear}"));
                document.Add(new Paragraph($"Group: {s.Group ?? "N/A"}"));
                document.Add(new Paragraph($"Status: {s.Status}"));
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> ExportAttendanceToPdfAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var attendance = await _unitOfWork.Attendances.GetAllAsync();
            var records = attendance.Where(a =>
                (!startDate.HasValue || a.Date >= startDate) &&
                (!endDate.HasValue || a.Date <= endDate));

            using var memoryStream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph("Attendance Export"));
            document.Add(new Paragraph($"Total Records: {records.Count()}"));
            document.Add(new Paragraph("\n"));

            foreach (var a in records)
            {
                document.Add(new Paragraph($"Student ID: {a.StudentId}"));
                document.Add(new Paragraph($"Date: {a.Date:yyyy-MM-dd}"));
                document.Add(new Paragraph($"Status: {a.Status}"));
                document.Add(new Paragraph($"Description: {a.Description}"));
                document.Add(new Paragraph("\n"));
            }

            document.Close();
            return memoryStream.ToArray();
        }
    }
}