using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using PdfDocument = iTextSharp.text.Document; // Alias for iTextSharp Document
using ExcelLicenseContext = OfficeOpenXml.LicenseContext; // Alias for EPPlus LicenseContext

namespace StThomasMission.Services.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ExcelPackage.LicenseContext = ExcelLicenseContext.NonCommercial;
        }

        public async Task<byte[]> GenerateStudentReportPdfAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new Exception("Student not found");

            var attendances = await _unitOfWork.Attendances.GetByStudentIdAsync(studentId);
            var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(studentId);
            var groupActivities = await _unitOfWork.GroupActivities.GetByGroupAsync(student.Group);

            using var memoryStream = new MemoryStream();
            var document = new PdfDocument();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph($"Student Report: {student.FamilyMember.FirstName} {student.FamilyMember.LastName}"));
            document.Add(new Paragraph($"Grade: {student.Grade} | Status: {student.Status}"));
            document.Add(new Paragraph("\nAttendance:"));
            foreach (var att in attendances)
            {
                document.Add(new Paragraph($"{att.Date:yyyy-MM-dd}: {(att.IsPresent ? "Present" : "Absent")} - {att.Description}"));
            }
            document.Add(new Paragraph("\nAssessments:"));
            foreach (var ass in assessments)
            {
                document.Add(new Paragraph($"{ass.Name} ({ass.Date:yyyy-MM-dd}): {ass.Marks}/{ass.TotalMarks} {(ass.IsMajor ? "(Major)" : "")}"));
            }
            document.Add(new Paragraph("\nGroup Points:"));
            int totalPoints = groupActivities.Sum(ga => ga.Points);
            document.Add(new Paragraph($"Total Points for {student.Group}: {totalPoints}"));

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerateClassReportPdfAsync(string grade)
        {
            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            using var memoryStream = new MemoryStream();
            var document = new PdfDocument();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph($"Class Report: {grade}"));
            document.Add(new Paragraph($"Total Students: {students.Count()}"));
            document.Add(new Paragraph("\nStudent Summary:"));
            foreach (var student in students)
            {
                var attendances = await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id);
                var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(student.Id);
                var attendanceRate = attendances.Any() ? (double)attendances.Count(a => a.IsPresent) / attendances.Count() * 100 : 0;
                var avgMarks = assessments.Any() ? assessments.Average(a => (double)a.Marks / a.TotalMarks * 100) : 0;

                document.Add(new Paragraph($"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}: Attendance: {attendanceRate:F2}% | Avg Marks: {avgMarks:F2}% | Status: {student.Status}"));
            }

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerateOverallCatechismReportPdfAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            using var memoryStream = new MemoryStream();
            var document = new PdfDocument();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph("Overall Catechism Report"));
            document.Add(new Paragraph($"Total Students: {students.Count()}"));
            document.Add(new Paragraph($"Graduated: {students.Count(s => s.Status == "Graduated")}"));
            document.Add(new Paragraph($"Active: {students.Count(s => s.Status == "Active")}"));
            document.Add(new Paragraph($"Migrated: {students.Count(s => s.Status == "Migrated")}"));

            var groups = students.Where(s => !string.IsNullOrEmpty(s.Group)).Select(s => s.Group).Distinct();
            document.Add(new Paragraph("\nGroup Rankings:"));
            foreach (var group in groups)
            {
                var activities = await _unitOfWork.GroupActivities.GetByGroupAsync(group);
                var totalPoints = activities.Sum(a => a.Points);
                document.Add(new Paragraph($"{group}: {totalPoints} points"));
            }

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerateFamilyReportPdfAsync()
        {
            var families = await _unitOfWork.Families.GetAllAsync();
            using var memoryStream = new MemoryStream();
            var document = new PdfDocument();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph("Family Report"));
            document.Add(new Paragraph($"Total Families: {families.Count()}"));
            document.Add(new Paragraph($"Registered: {families.Count(f => f.IsRegistered)}"));
            document.Add(new Paragraph($"Unregistered: {families.Count(f => !f.IsRegistered)}"));
            document.Add(new Paragraph($"Migrated: {families.Count(f => f.Status == "Migrated")}"));

            document.Add(new Paragraph("\nWard Distribution:"));
            var wards = families.Select(f => f.Ward).Distinct();
            foreach (var ward in wards)
            {
                document.Add(new Paragraph($"{ward}: {families.Count(f => f.Ward == ward)} families"));
            }

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerateStudentReportExcelAsync(int studentId)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new Exception("Student not found");

            var attendances = await _unitOfWork.Attendances.GetByStudentIdAsync(studentId);
            var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(studentId);
            var groupActivities = await _unitOfWork.GroupActivities.GetByGroupAsync(student.Group);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Student Report");
            worksheet.Cells[1, 1].Value = "Student Report";
            worksheet.Cells[2, 1].Value = "Name";
            worksheet.Cells[2, 2].Value = $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}";
            worksheet.Cells[3, 1].Value = "Grade";
            worksheet.Cells[3, 2].Value = student.Grade;
            worksheet.Cells[4, 1].Value = "Status";
            worksheet.Cells[4, 2].Value = student.Status;

            worksheet.Cells[6, 1].Value = "Attendance";
            worksheet.Cells[7, 1].Value = "Date";
            worksheet.Cells[7, 2].Value = "Status";
            worksheet.Cells[7, 3].Value = "Description";
            int row = 8;
            foreach (var att in attendances)
            {
                worksheet.Cells[row, 1].Value = att.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 2].Value = att.IsPresent ? "Present" : "Absent";
                worksheet.Cells[row, 3].Value = att.Description;
                row++;
            }

            worksheet.Cells[row + 1, 1].Value = "Assessments";
            worksheet.Cells[row + 2, 1].Value = "Name";
            worksheet.Cells[row + 2, 2].Value = "Date";
            worksheet.Cells[row + 2, 3].Value = "Marks";
            worksheet.Cells[row + 2, 4].Value = "Total Marks";
            worksheet.Cells[row + 2, 5].Value = "Type";
            row += 3;
            foreach (var ass in assessments)
            {
                worksheet.Cells[row, 1].Value = ass.Name;
                worksheet.Cells[row, 2].Value = ass.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 3].Value = ass.Marks;
                worksheet.Cells[row, 4].Value = ass.TotalMarks;
                worksheet.Cells[row, 5].Value = ass.IsMajor ? "Major" : "Minor";
                row++;
            }

            worksheet.Cells[row + 1, 1].Value = "Group Points";
            worksheet.Cells[row + 2, 1].Value = "Group";
            worksheet.Cells[row + 2, 2].Value = "Total Points";
            worksheet.Cells[row + 3, 1].Value = student.Group;
            worksheet.Cells[row + 3, 2].Value = groupActivities.Sum(ga => ga.Points);

            return package.GetAsByteArray();
        }

        public async Task<byte[]> GenerateClassReportExcelAsync(string grade)
        {
            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Class Report");
            worksheet.Cells[1, 1].Value = $"Class Report: {grade}";
            worksheet.Cells[2, 1].Value = "Total Students";
            worksheet.Cells[2, 2].Value = students.Count();

            worksheet.Cells[4, 1].Value = "Student Name";
            worksheet.Cells[4, 2].Value = "Attendance Rate (%)";
            worksheet.Cells[4, 3].Value = "Average Marks (%)";
            worksheet.Cells[4, 4].Value = "Status";
            int row = 5;
            foreach (var student in students)
            {
                var attendances = await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id);
                var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(student.Id);
                var attendanceRate = attendances.Any() ? (double)attendances.Count(a => a.IsPresent) / attendances.Count() * 100 : 0;
                var avgMarks = assessments.Any() ? assessments.Average(a => (double)a.Marks / a.TotalMarks * 100) : 0;

                worksheet.Cells[row, 1].Value = $"{student.FamilyMember.FirstName} {student.FamilyMember.LastName}";
                worksheet.Cells[row, 2].Value = attendanceRate;
                worksheet.Cells[row, 3].Value = avgMarks;
                worksheet.Cells[row, 4].Value = student.Status;
                row++;
            }

            return package.GetAsByteArray();
        }

        public async Task<byte[]> GenerateOverallCatechismReportExcelAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Overall Catechism Report");
            worksheet.Cells[1, 1].Value = "Overall Catechism Report";
            worksheet.Cells[2, 1].Value = "Total Students";
            worksheet.Cells[2, 2].Value = students.Count();
            worksheet.Cells[3, 1].Value = "Graduated";
            worksheet.Cells[3, 2].Value = students.Count(s => s.Status == "Graduated");
            worksheet.Cells[4, 1].Value = "Active";
            worksheet.Cells[4, 2].Value = students.Count(s => s.Status == "Active");
            worksheet.Cells[5, 1].Value = "Migrated";
            worksheet.Cells[5, 2].Value = students.Count(s => s.Status == "Migrated");

            var groups = students.Where(s => !string.IsNullOrEmpty(s.Group)).Select(s => s.Group).Distinct();
            worksheet.Cells[7, 1].Value = "Group Rankings";
            worksheet.Cells[8, 1].Value = "Group";
            worksheet.Cells[8, 2].Value = "Total Points";
            int row = 9;
            foreach (var group in groups)
            {
                var activities = await _unitOfWork.GroupActivities.GetByGroupAsync(group);
                worksheet.Cells[row, 1].Value = group;
                worksheet.Cells[row, 2].Value = activities.Sum(a => a.Points);
                row++;
            }

            return package.GetAsByteArray();
        }

        public async Task<byte[]> GenerateFamilyReportExcelAsync()
        {
            var families = await _unitOfWork.Families.GetAllAsync();
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Family Report");
            worksheet.Cells[1, 1].Value = "Family Report";
            worksheet.Cells[2, 1].Value = "Total Families";
            worksheet.Cells[2, 2].Value = families.Count();
            worksheet.Cells[3, 1].Value = "Registered";
            worksheet.Cells[3, 2].Value = families.Count(f => f.IsRegistered);
            worksheet.Cells[4, 1].Value = "Unregistered";
            worksheet.Cells[4, 2].Value = families.Count(f => !f.IsRegistered);
            worksheet.Cells[5, 1].Value = "Migrated";
            worksheet.Cells[5, 2].Value = families.Count(f => f.Status == "Migrated");

            var wards = families.Select(f => f.Ward).Distinct();
            worksheet.Cells[7, 1].Value = "Ward Distribution";
            worksheet.Cells[8, 1].Value = "Ward";
            worksheet.Cells[8, 2].Value = "Family Count";
            int row = 9;
            foreach (var ward in wards)
            {
                worksheet.Cells[row, 1].Value = ward;
                worksheet.Cells[row, 2].Value = families.Count(f => f.Ward == ward);
                row++;
            }

            return package.GetAsByteArray();
        }
    }
}