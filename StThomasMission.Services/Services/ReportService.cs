using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class ReportService : IReportingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> GenerateStudentReportAsync(int studentId, string format)
        {
            if (!new[] { "PDF", "Excel" }.Contains(format)) throw new ArgumentException("Format must be 'PDF' or 'Excel'.", nameof(format));

            var student = await _unitOfWork.Students.GetByIdAsync(studentId);
            if (student == null) throw new ArgumentException("Student not found.", nameof(studentId));

            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            var attendances = await (_unitOfWork.Attendances as IAttendanceRepository)!.GetByStudentIdAsync(studentId);
            var assessments = await (_unitOfWork.Assessments as IAssessmentRepository)!.GetByStudentIdAsync(studentId);
            var groupActivities = await (_unitOfWork.GroupActivities as IGroupActivityRepository)!.GetByGroupAsync(student.Group);

            // Replace enum checks with correct types
            if (format == "PDF")
            {
                using var memoryStream = new MemoryStream();
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph($"Student Report: {familyMember.FirstName} {familyMember.LastName}"));
                document.Add(new Paragraph($"Grade: {student.Grade} | Academic Year: {student.AcademicYear} | Status: {student.Status}"));
                if (student.Status == StudentStatus.Migrated)
                    document.Add(new Paragraph($"Migrated To: {student.MigratedTo}"));

                document.Add(new Paragraph("\nAttendance:"));
                foreach (var att in attendances)
                {
                    var status = att.Status == AttendanceStatus.Present ? "Present" : "Absent";
                    document.Add(new Paragraph($"{att.Date:yyyy-MM-dd}: {status} - {att.Description}"));
                }

                document.Add(new Paragraph("\nAssessments:"));
                foreach (var ass in assessments)
                {
                    var type = ass.Type == AssessmentType.SemesterExam ? "(Major)" : "";
                    document.Add(new Paragraph($"{ass.Name} ({ass.Date:yyyy-MM-dd}): {ass.Marks}/{ass.TotalMarks} {type}"));
                }

                document.Add(new Paragraph("\nGroup Points:"));
                int totalPoints = groupActivities.Sum(ga => ga.Points);
                document.Add(new Paragraph($"Total Points for {student.Group ?? "N/A"}: {totalPoints}"));

                document.Close();
                return memoryStream.ToArray();
            }
            else // Excel
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Student Report");
                worksheet.Cells[1, 1].Value = "Student Report";
                worksheet.Cells[2, 1].Value = "Name";
                worksheet.Cells[2, 2].Value = $"{familyMember.FirstName} {familyMember.LastName}";
                worksheet.Cells[3, 1].Value = "Grade";
                worksheet.Cells[3, 2].Value = student.Grade;
                worksheet.Cells[4, 1].Value = "Academic Year";
                worksheet.Cells[4, 2].Value = student.AcademicYear;
                worksheet.Cells[5, 1].Value = "Status";
                worksheet.Cells[5, 2].Value = student.Status.ToString();
                if (student.Status == StudentStatus.Migrated)
                {
                    worksheet.Cells[6, 1].Value = "Migrated To";
                    worksheet.Cells[6, 2].Value = student.MigratedTo;
                }

                worksheet.Cells[8, 1].Value = "Attendance";
                worksheet.Cells[9, 1].Value = "Date";
                worksheet.Cells[9, 2].Value = "Status";
                worksheet.Cells[9, 3].Value = "Description";
                int row = 10;
                foreach (var att in attendances)
                {
                    worksheet.Cells[row, 1].Value = att.Date.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 2].Value = att.Status == AttendanceStatus.Present ? "Present" : "Absent";
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
                    worksheet.Cells[row, 5].Value = ass.Type == AssessmentType.SemesterExam ? "Major" : "Minor";
                    row++;
                }

                worksheet.Cells[row + 1, 1].Value = "Group Points";
                worksheet.Cells[row + 2, 1].Value = "Group";
                worksheet.Cells[row + 2, 2].Value = "Total Points";
                worksheet.Cells[row + 3, 1].Value = student.Group ?? "N/A";
                worksheet.Cells[row + 3, 2].Value = groupActivities.Sum(ga => ga.Points);

                return package.GetAsByteArray();
            }
        }

       

        private byte[] GenerateStudentPdf(Student student, FamilyMember familyMember,
            IEnumerable<Attendance> attendances, IEnumerable<Assessment> assessments,
            IEnumerable<GroupActivity> groupActivities)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph($"Student Report: {familyMember.FirstName} {familyMember.LastName}"));
            document.Add(new Paragraph($"Grade: {student.Grade} | Academic Year: {student.AcademicYear} | Status: {student.Status}"));
            if (student.Status == StudentStatus.Migrated)
                document.Add(new Paragraph($"Migrated To: {student.MigratedTo}"));

            document.Add(new Paragraph("\nAttendance:"));
            foreach (var att in attendances)
                document.Add(new Paragraph($"{att.Date:yyyy-MM-dd}: {(att.Status == AttendanceStatus.Present ? "Present" : "Absent")} - {att.Description}"));

            document.Add(new Paragraph("\nAssessments:"));
            foreach (var ass in assessments)
                document.Add(new Paragraph($"{ass.Name} ({ass.Date:yyyy-MM-dd}): {ass.Marks}/{ass.TotalMarks} {(ass.Type == AssessmentType.SemesterExam ? "(Major)" : "")}"));

            document.Add(new Paragraph("\nGroup Points:"));
            document.Add(new Paragraph($"Total Points for {student.Group ?? "N/A"}: {groupActivities.Sum(ga => ga.Points)}"));

            document.Close();
            return memoryStream.ToArray();
        }

        private byte[] GenerateStudentExcel(Student student, FamilyMember familyMember,
            IEnumerable<Attendance> attendances, IEnumerable<Assessment> assessments,
            IEnumerable<GroupActivity> groupActivities)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Student Report");

            worksheet.Cells[1, 1].Value = "Student Report";
            worksheet.Cells[2, 1].Value = "Name";
            worksheet.Cells[2, 2].Value = $"{familyMember.FirstName} {familyMember.LastName}";
            worksheet.Cells[3, 1].Value = "Grade";
            worksheet.Cells[3, 2].Value = student.Grade;
            worksheet.Cells[4, 1].Value = "Academic Year";
            worksheet.Cells[4, 2].Value = student.AcademicYear;
            worksheet.Cells[5, 1].Value = "Status";
            worksheet.Cells[5, 2].Value = student.Status.ToString();
            if (student.Status == StudentStatus.Migrated)
            {
                worksheet.Cells[6, 1].Value = "Migrated To";
                worksheet.Cells[6, 2].Value = student.MigratedTo;
            }

            int row = 8;
            worksheet.Cells[row++, 1].Value = "Attendance";
            worksheet.Cells[row++, 1].Value = "Date";
            worksheet.Cells[row - 1, 2].Value = "Status";
            worksheet.Cells[row - 1, 3].Value = "Description";
            foreach (var att in attendances)
            {
                worksheet.Cells[row, 1].Value = att.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 2].Value = att.Status == AttendanceStatus.Present ? "Present" : "Absent";
                worksheet.Cells[row, 3].Value = att.Description;
                row++;
            }

            row++;
            worksheet.Cells[row++, 1].Value = "Assessments";
            worksheet.Cells[row++, 1].Value = "Name";
            worksheet.Cells[row - 1, 2].Value = "Date";
            worksheet.Cells[row - 1, 3].Value = "Marks";
            worksheet.Cells[row - 1, 4].Value = "Total Marks";
            worksheet.Cells[row - 1, 5].Value = "Type";
            foreach (var ass in assessments)
            {
                worksheet.Cells[row, 1].Value = ass.Name;
                worksheet.Cells[row, 2].Value = ass.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 3].Value = ass.Marks;
                worksheet.Cells[row, 4].Value = ass.TotalMarks;
                worksheet.Cells[row, 5].Value = ass.Type == AssessmentType.SemesterExam ? "Major" : "Minor";
                row++;
            }

            row++;
            worksheet.Cells[row++, 1].Value = "Group Points";
            worksheet.Cells[row, 1].Value = student.Group ?? "N/A";
            worksheet.Cells[row, 2].Value = groupActivities.Sum(ga => ga.Points);

            return package.GetAsByteArray();
        }
        public async Task<byte[]> GenerateClassReportAsync(string grade, int academicYear, string format)
        {
            if (!new[] { "PDF", "Excel" }.Contains(format)) throw new ArgumentException("Format must be 'PDF' or 'Excel'.", nameof(format));
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$")) throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));

            var students = (await _unitOfWork.Students.GetByGradeAsync(grade))
                .Where(s => s.AcademicYear == academicYear);

            if (format == "PDF")
            {
                using var memoryStream = new MemoryStream();
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph($"Class Report: {grade}, Academic Year: {academicYear}"));
                document.Add(new Paragraph($"Total Students: {students.Count()}"));
                document.Add(new Paragraph("\nStudent Summary:"));

                foreach (var student in students)
                {
                    var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                    var attendances = await (_unitOfWork.Attendances as IAttendanceRepository)!.GetByStudentIdAsync(student.Id);
                    var assessments = await (_unitOfWork.Assessments as IAssessmentRepository)!.GetByStudentIdAsync(student.Id);

                    var attendanceRate = attendances.Any() ? (double)attendances.Count(a => a.Status == AttendanceStatus.Present) / attendances.Count() * 100 : 0;
                    var avgMarks = assessments.Any() ? assessments.Average(a => (double)a.Marks / a.TotalMarks * 100) : 0;

                    document.Add(new Paragraph($"{familyMember.FirstName} {familyMember.LastName}: Attendance: {attendanceRate:F2}% | Avg Marks: {avgMarks:F2}% | Status: {student.Status}"));
                    if (student.Status == StudentStatus.Migrated)
                    {
                        document.Add(new Paragraph($"  Migrated To: {student.MigratedTo}"));
                    }
                }

                document.Close();
                return memoryStream.ToArray();
            }
            else // Excel
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Class Report");
                worksheet.Cells[1, 1].Value = $"Class Report: {grade}, Academic Year: {academicYear}";
                worksheet.Cells[2, 1].Value = "Total Students";
                worksheet.Cells[2, 2].Value = students.Count();

                worksheet.Cells[4, 1].Value = "Student Name";
                worksheet.Cells[4, 2].Value = "Attendance Rate (%)";
                worksheet.Cells[4, 3].Value = "Average Marks (%)";
                worksheet.Cells[4, 4].Value = "Status";
                worksheet.Cells[4, 5].Value = "Migrated To";

                int row = 5;
                foreach (var student in students)
                {
                    var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                    var attendances = await (_unitOfWork.Attendances as IAttendanceRepository)!.GetByStudentIdAsync(student.Id);
                    var assessments = await (_unitOfWork.Assessments as IAssessmentRepository)!.GetByStudentIdAsync(student.Id);

                    var attendanceRate = attendances.Any() ? (double)attendances.Count(a => a.Status == AttendanceStatus.Present) / attendances.Count() * 100 : 0;
                    var avgMarks = assessments.Any() ? assessments.Average(a => (double)a.Marks / a.TotalMarks * 100) : 0;

                    worksheet.Cells[row, 1].Value = $"{familyMember.FirstName} {familyMember.LastName}";
                    worksheet.Cells[row, 2].Value = attendanceRate;
                    worksheet.Cells[row, 3].Value = avgMarks;
                    worksheet.Cells[row, 4].Value = student.Status.ToString();
                    worksheet.Cells[row, 5].Value = student.Status == StudentStatus.Migrated ? student.MigratedTo : "";
                    row++;
                }

                return package.GetAsByteArray();
            }
        }
        public async Task<byte[]> GenerateCatechismReportAsync(int academicYear, string format)
        {
            if (!new[] { "PDF", "Excel" }.Contains(format)) throw new ArgumentException("Format must be 'PDF' or 'Excel'.", nameof(format));

            var students = (await _unitOfWork.Students.GetAllAsync())
                .Where(s => s.AcademicYear == academicYear);

            if (format == "PDF")
            {
                using var memoryStream = new MemoryStream();
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph($"Catechism Report: Academic Year {academicYear}"));
                document.Add(new Paragraph($"Total Students: {students.Count()}"));
                document.Add(new Paragraph($"Graduated: {students.Count(s => s.Status == StudentStatus.Graduated)}"));
                document.Add(new Paragraph($"Active: {students.Count(s => s.Status == StudentStatus.Active)}"));
                document.Add(new Paragraph($"Migrated: {students.Count(s => s.Status == StudentStatus.Migrated)}"));

                document.Add(new Paragraph("\nTrend Analysis (Students by Grade):"));
                var studentsByGrade = students.GroupBy(s => s.Grade)
                    .OrderBy(g => g.Key)
                    .Select(g => new { Grade = g.Key, Count = g.Count() });

                foreach (var gradeData in studentsByGrade)
                {
                    document.Add(new Paragraph($"{gradeData.Grade}: {gradeData.Count} students"));
                }

                document.Close();
                return memoryStream.ToArray();
            }
            else // Excel
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Catechism Report");
                worksheet.Cells[1, 1].Value = $"Catechism Report: Academic Year {academicYear}";
                worksheet.Cells[2, 1].Value = "Total Students";
                worksheet.Cells[2, 2].Value = students.Count();
                worksheet.Cells[3, 1].Value = "Graduated";
                worksheet.Cells[3, 2].Value = students.Count(s => s.Status == StudentStatus.Graduated);
                worksheet.Cells[4, 1].Value = "Active";
                worksheet.Cells[4, 2].Value = students.Count(s => s.Status == StudentStatus.Active);
                worksheet.Cells[5, 1].Value = "Migrated";
                worksheet.Cells[5, 2].Value = students.Count(s => s.Status == StudentStatus.Migrated);

                worksheet.Cells[7, 1].Value = "Students by Grade";
                worksheet.Cells[8, 1].Value = "Grade";
                worksheet.Cells[8, 2].Value = "Count";

                int row = 9;
                var studentsByGrade = students.GroupBy(s => s.Grade)
                    .OrderBy(g => g.Key)
                    .Select(g => new { Grade = g.Key, Count = g.Count() });

                foreach (var gradeData in studentsByGrade)
                {
                    worksheet.Cells[row, 1].Value = gradeData.Grade;
                    worksheet.Cells[row, 2].Value = gradeData.Count;
                    row++;
                }

                return package.GetAsByteArray();
            }
        }
        public async Task<byte[]> GenerateFamilyReportAsync(string? ward, string? status, string format)
        {
            if (!new[] { "PDF", "Excel" }.Contains(format)) throw new ArgumentException("Format must be 'PDF' or 'Excel'.", nameof(format));

            var families = await _unitOfWork.Families.GetAllAsync();
            if (ward != null) families = families.Where(f => f.Ward == ward);
            if (status != null && Enum.TryParse<FamilyStatus>(status, true, out var parsedStatus))
                families = families.Where(f => f.Status == parsedStatus);

            if (format == "PDF")
            {
                using var memoryStream = new MemoryStream();
                var document = new Document();
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph("Family Report"));
                document.Add(new Paragraph($"Total Families: {families.Count()}"));
                document.Add(new Paragraph($"Registered: {families.Count(f => f.IsRegistered)}"));
                document.Add(new Paragraph($"Unregistered: {families.Count(f => !f.IsRegistered)}"));
                document.Add(new Paragraph($"Migrated: {families.Count(f => f.Status == FamilyStatus.Migrated)}"));

                document.Add(new Paragraph("\nWard Distribution:"));
                var wards = families.Select(f => f.Ward).Distinct();
                foreach (var w in wards)
                {
                    document.Add(new Paragraph($"{w}: {families.Count(f => f.Ward == w)} families"));
                }

                document.Close();
                return memoryStream.ToArray();
            }
            else // Excel
            {
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
                worksheet.Cells[5, 2].Value = families.Count(f => f.Status == FamilyStatus.Migrated);

                var wards = families.Select(f => f.Ward).Distinct();
                worksheet.Cells[7, 1].Value = "Ward Distribution";
                worksheet.Cells[8, 1].Value = "Ward";
                worksheet.Cells[8, 2].Value = "Family Count";

                int row = 9;
                foreach (var w in wards)
                {
                    worksheet.Cells[row, 1].Value = w;
                    worksheet.Cells[row, 2].Value = families.Count(f => f.Ward == w);
                    row++;
                }

                return package.GetAsByteArray();
            }
        }

    }
}
