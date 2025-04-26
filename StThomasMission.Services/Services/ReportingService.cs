using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System.Text.RegularExpressions;

namespace StThomasMission.Services
{
    public class ReportingService : IReportingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IFamilyService _familyService;
        private readonly IWardService _wardService;

        public ReportingService(IUnitOfWork unitOfWork, IStudentService studentService, IFamilyService familyService, IWardService wardService)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _familyService = familyService;
            _wardService = wardService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateStudentReportAsync(int studentId, ReportFormat format)
        {
            var student = await _studentService.GetStudentByIdAsync(studentId);
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
            var group = await _unitOfWork.Groups.GetByIdAsync(student.GroupId);
            var attendances = await _unitOfWork.Attendances.GetByStudentIdAsync(studentId);
            var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(studentId);
            var groupActivities = await _unitOfWork.StudentGroupActivities.GetByStudentIdAsync(studentId);

            if (format == ReportFormat.Pdf)
            {
                return await GenerateStudentPdfAsync(student, familyMember, group, attendances, assessments, groupActivities);
            }
            else // Excel
            {
                return await GenerateStudentExcelAsync(student, familyMember, group, attendances, assessments, groupActivities);
            }
        }

        public async Task<byte[]> GenerateClassReportAsync(string grade, int academicYear, ReportFormat format)
        {
            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                throw new ArgumentException("Grade must be in format 'Year X'.", nameof(grade));
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            var students = await _unitOfWork.Students.GetByGradeAsync(grade);
            students = students.Where(s => s.AcademicYear == academicYear).ToList();

            if (format == ReportFormat.Pdf)
            {
                return await GenerateClassPdfAsync(grade, academicYear, students);
            }
            else // Excel
            {
                return await GenerateClassExcelAsync(grade, academicYear, students);
            }
        }

        public async Task<byte[]> GenerateCatechismReportAsync(int academicYear, ReportFormat format)
        {
            if (academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                throw new ArgumentException("Invalid academic year.", nameof(academicYear));

            var students = await _unitOfWork.Students.GetAsync(s => s.AcademicYear == academicYear);

            if (format == ReportFormat.Pdf)
            {
                return await GenerateCatechismPdfAsync(academicYear, students);
            }
            else // Excel
            {
                return await GenerateCatechismExcelAsync(academicYear, students);
            }
        }

        public async Task<byte[]> GenerateFamilyReportAsync(int? wardId, FamilyStatus? status, ReportFormat format)
        {
            IEnumerable<Family> families;
            if (wardId.HasValue)
            {
                await _wardService.GetWardByIdAsync(wardId.Value);
                families = await _unitOfWork.Families.GetByWardAsync(wardId.Value);
            }
            else if (status.HasValue)
            {
                families = await _unitOfWork.Families.GetByStatusAsync(status.Value);
            }
            else
            {
                families = await _unitOfWork.Families.GetAllAsync();
            }

            if (format == ReportFormat.Pdf)
            {
                return await GenerateFamilyPdfAsync(families);
            }
            else // Excel
            {
                return await GenerateFamilyExcelAsync(families);
            }
        }

        public async Task<byte[]> GenerateWardReportAsync(int wardId, ReportFormat format)
        {
            var ward = await _wardService.GetWardByIdAsync(wardId);
            var families = await _unitOfWork.Families.GetByWardAsync(wardId);

            if (format == ReportFormat.Pdf)
            {
                return await GenerateWardPdfAsync(ward, families);
            }
            else // Excel
            {
                return await GenerateWardExcelAsync(ward, families);
            }
        }

        public async Task<DashboardSummaryDto> GenerateDashboardSummaryAsync()
        {
            return await new DashboardService(_unitOfWork).GetDashboardSummaryAsync();
        }

        private async Task<byte[]> GenerateStudentPdfAsync(Student student, FamilyMember familyMember, StThomasMission.Core.Entities.Group group, IEnumerable<Attendance> attendances, IEnumerable<Assessment> assessments, IEnumerable<StudentGroupActivity> groupActivities)
        {
            return await Task.Run(() =>
            {
                var document = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Content().Column(column =>
                        {
                            column.Item().Text($"Student Report: {familyMember.FullName}").FontSize(16).Bold();
                            column.Item().Text($"Grade: {student.Grade} | Academic Year: {student.AcademicYear} | Group: {group.Name} | Status: {student.Status}");
                            if (student.Status == StudentStatus.Migrated)
                                column.Item().Text($"Migrated To: {student.MigratedTo}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10).Text("Attendance").FontSize(14).Bold();
                            foreach (var att in attendances)
                            {
                                column.Item().Text($"{att.Date:yyyy-MM-dd}: {att.Status} - {att.Description}");
                            }

                            column.Item().PaddingTop(10).Text("Assessments").FontSize(14).Bold();
                            foreach (var ass in assessments)
                            {
                                column.Item().Text($"{ass.Name} ({ass.Date:yyyy-MM-dd}): {ass.Marks}/{ass.TotalMarks} ({ass.Type})");
                            }

                            column.Item().PaddingTop(10).Text("Group Activities").FontSize(14).Bold();
                            foreach (var sga in groupActivities)
                            {
                                var activity = _unitOfWork.GroupActivities.GetByIdAsync(sga.GroupActivityId).Result;
                                column.Item().Text($"{activity.Name} ({sga.ParticipationDate:yyyy-MM-dd}): {sga.PointsEarned} points");
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        private async Task<byte[]> GenerateStudentExcelAsync(Student student, FamilyMember familyMember, StThomasMission.Core.Entities.Group group, IEnumerable<Attendance> attendances, IEnumerable<Assessment> assessments, IEnumerable<StudentGroupActivity> groupActivities)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Student Report");

            worksheet.Cells[1, 1].Value = "Student Report";
            worksheet.Cells[2, 1].Value = "Name";
            worksheet.Cells[2, 2].Value = familyMember.FullName;
            worksheet.Cells[3, 1].Value = "Grade";
            worksheet.Cells[3, 2].Value = student.Grade;
            worksheet.Cells[4, 1].Value = "Academic Year";
            worksheet.Cells[4, 2].Value = student.AcademicYear;
            worksheet.Cells[5, 1].Value = "Group";
            worksheet.Cells[5, 2].Value = group.Name;
            worksheet.Cells[6, 1].Value = "Status";
            worksheet.Cells[6, 2].Value = student.Status.ToString();
            if (student.Status == StudentStatus.Migrated)
            {
                worksheet.Cells[7, 1].Value = "Migrated To";
                worksheet.Cells[7, 2].Value = student.MigratedTo;
            }

            worksheet.Cells[9, 1].Value = "Attendance";
            worksheet.Cells[10, 1].Value = "Date";
            worksheet.Cells[10, 2].Value = "Status";
            worksheet.Cells[10, 3].Value = "Description";
            int row = 11;
            foreach (var att in attendances)
            {
                worksheet.Cells[row, 1].Value = att.Date.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 2].Value = att.Status.ToString();
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
                worksheet.Cells[row, 5].Value = ass.Type.ToString();
                row++;
            }

            worksheet.Cells[row + 1, 1].Value = "Group Activities";
            worksheet.Cells[row + 2, 1].Value = "Activity Name";
            worksheet.Cells[row + 2, 2].Value = "Participation Date";
            worksheet.Cells[row + 2, 3].Value = "Points Earned";
            row += 3;
            foreach (var sga in groupActivities)
            {
                var activity = await _unitOfWork.GroupActivities.GetByIdAsync(sga.GroupActivityId);
                worksheet.Cells[row, 1].Value = activity?.Name ?? "Unknown";
                worksheet.Cells[row, 2].Value = sga.ParticipationDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 3].Value = sga.PointsEarned;
                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }
        private async Task<byte[]> GenerateClassPdfAsync(string grade, int academicYear, IEnumerable<Student> students)
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
                            column.Item().Text($"Class Report: {grade}, Academic Year: {academicYear}").FontSize(16).Bold();
                            column.Item().Text($"Total Students: {students.Count()}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10).Text("Student Summary").FontSize(14).Bold();
                            foreach (var student in students)
                            {
                                var familyMemberTask = _unitOfWork.FamilyMembers.GetByIdAsync(student.FamilyMemberId);
                                familyMemberTask.Wait();
                                var familyMember = familyMemberTask.Result;

                                var attendancesTask = _unitOfWork.Attendances.GetByStudentIdAsync(student.Id);
                                attendancesTask.Wait();
                                var attendances = attendancesTask.Result;

                                var assessmentsTask = _unitOfWork.Assessments.GetByStudentIdAsync(student.Id);
                                assessmentsTask.Wait();
                                var assessments = assessmentsTask.Result;

                                var attendanceRate = attendances.Any() ? (double)attendances.Count(a => a.Status == AttendanceStatus.Present) / attendances.Count() * 100 : 0;
                                var avgMarks = assessments.Any() ? assessments.Average(a => (double)a.Marks / a.TotalMarks * 100) : 0;

                                column.Item().PaddingVertical(5).Text($"{familyMember.FullName}: Attendance: {attendanceRate:F2}% | Avg Marks: {avgMarks:F2}% | Status: {student.Status}");
                                if (student.Status == StudentStatus.Migrated)
                                    column.Item().Text($"  Migrated To: {student.MigratedTo}");
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        private async Task<byte[]> GenerateClassExcelAsync(string grade, int academicYear, IEnumerable<Student> students)
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
                var attendances = await _unitOfWork.Attendances.GetByStudentIdAsync(student.Id);
                var assessments = await _unitOfWork.Assessments.GetByStudentIdAsync(student.Id);

                var attendanceRate = attendances.Any() ? (double)attendances.Count(a => a.Status == AttendanceStatus.Present) / attendances.Count() * 100 : 0;
                var avgMarks = assessments.Any() ? assessments.Average(a => (double)a.Marks / a.TotalMarks * 100) : 0;

                worksheet.Cells[row, 1].Value = familyMember?.FullName ?? "Unknown";
                worksheet.Cells[row, 2].Value = attendanceRate;
                worksheet.Cells[row, 3].Value = avgMarks;
                worksheet.Cells[row, 4].Value = student.Status.ToString();
                worksheet.Cells[row, 5].Value = student.Status == StudentStatus.Migrated ? student.MigratedTo : "";
                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task<byte[]> GenerateCatechismPdfAsync(int academicYear, IEnumerable<Student> students)
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
                            column.Item().Text($"Catechism Report: Academic Year {academicYear}").FontSize(16).Bold();
                            column.Item().Text($"Total Students: {students.Count()}");
                            column.Item().Text($"Graduated: {students.Count(s => s.Status == StudentStatus.Graduated)}");
                            column.Item().Text($"Active: {students.Count(s => s.Status == StudentStatus.Active)}");
                            column.Item().Text($"Migrated: {students.Count(s => s.Status == StudentStatus.Migrated)}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10).Text("Students by Grade").FontSize(14).Bold();
                            var studentsByGrade = students.GroupBy(s => s.Grade)
                                .OrderBy(g => g.Key)
                                .Select(g => new { Grade = g.Key, Count = g.Count() });

                            foreach (var gradeData in studentsByGrade)
                            {
                                column.Item().Text($"{gradeData.Grade}: {gradeData.Count} students");
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        private async Task<byte[]> GenerateCatechismExcelAsync(int academicYear, IEnumerable<Student> students)
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

            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task<byte[]> GenerateFamilyPdfAsync(IEnumerable<Family> families)
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
                            column.Item().Text("Family Report").FontSize(16).Bold();
                            column.Item().Text($"Total Families: {families.Count()}");
                            column.Item().Text($"Registered: {families.Count(f => f.IsRegistered)}");
                            column.Item().Text($"Unregistered: {families.Count(f => !f.IsRegistered)}");
                            column.Item().Text($"Migrated: {families.Count(f => f.Status == FamilyStatus.Migrated)}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10).Text("Ward Distribution").FontSize(14).Bold();
                            var wards = _unitOfWork.Wards.GetAllAsync().Result.Select(w => w.Id);
                            foreach (var w in wards)
                            {
                                var count = families.Count(f => f.WardId == w);
                                var ward = _unitOfWork.Wards.GetByIdAsync(w).Result;
                                column.Item().Text($"{ward.Name}: {count} families");
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        private async Task<byte[]> GenerateFamilyExcelAsync(IEnumerable<Family> families)
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

            worksheet.Cells[7, 1].Value = "Ward Distribution";
            worksheet.Cells[8, 1].Value = "Ward";
            worksheet.Cells[8, 2].Value = "Family Count";

            int row = 9;
            var wards = await _unitOfWork.Wards.GetAllAsync();
            foreach (var w in wards)
            {
                var count = families.Count(f => f.WardId == w.Id);
                worksheet.Cells[row, 1].Value = w.Name;
                worksheet.Cells[row, 2].Value = count;
                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }

        private async Task<byte[]> GenerateWardPdfAsync(Ward ward, IEnumerable<Family> families)
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
                            column.Item().Text($"Ward Report: {ward.Name}").FontSize(16).Bold();
                            column.Item().Text($"Total Families: {families.Count()}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10).Text("Families").FontSize(14).Bold();
                            foreach (var f in families)
                            {
                                column.Item().PaddingVertical(5).Text(text =>
                                {
                                    text.Span(f.FamilyName);
                                    text.Span(f.IsRegistered ? $" (Reg: {f.ChurchRegistrationNumber})" : $" (Temp: {f.TemporaryID})");
                                    text.Span($" - Status: {f.Status}");
                                });
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        private async Task<byte[]> GenerateWardExcelAsync(Ward ward, IEnumerable<Family> families)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Ward Report");

            worksheet.Cells[1, 1].Value = $"Ward Report: {ward.Name}";
            worksheet.Cells[2, 1].Value = "Total Families";
            worksheet.Cells[2, 2].Value = families.Count();

            worksheet.Cells[4, 1].Value = "Family Name";
            worksheet.Cells[4, 2].Value = "Registration Status";
            worksheet.Cells[4, 3].Value = "Status";

            int row = 5;
            foreach (var f in families)
            {
                worksheet.Cells[row, 1].Value = f.FamilyName;
                worksheet.Cells[row, 2].Value = f.IsRegistered ? f.ChurchRegistrationNumber : f.TemporaryID;
                worksheet.Cells[row, 3].Value = f.Status.ToString();
                row++;
            }

            return await Task.FromResult(package.GetAsByteArray());
        }
    }
}