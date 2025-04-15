using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class ImportService : IImportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ImportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<bool> ImportFamiliesAndStudentsAsync(Stream fileStream)
        {
            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        // Read family details
                        string familyName = worksheet.Cells[row, 1].Text ?? string.Empty;
                        string ward = worksheet.Cells[row, 2].Text ?? string.Empty;
                        bool isRegistered = bool.TryParse(worksheet.Cells[row, 3].Text, out bool reg) && reg;
                        string? churchRegNumber = isRegistered ? $"10802{row:D4}" : null;
                        string? tempId = isRegistered ? null : $"TMP-{row:D4}";

                        var family = new Family
                        {
                            FamilyName = familyName,
                            Ward = ward,
                            IsRegistered = isRegistered,
                            ChurchRegistrationNumber = churchRegNumber,
                            TemporaryID = tempId,
                            Status = "Active",
                            CreatedDate = DateTime.UtcNow
                        };
                        await _unitOfWork.Families.AddAsync(family);
                        await _unitOfWork.CompleteAsync();

                        // Read family member details
                        string firstName = worksheet.Cells[row, 4].Text ?? string.Empty;
                        string lastName = worksheet.Cells[row, 5].Text ?? string.Empty;
                        DateTime dob = DateTime.TryParse(worksheet.Cells[row, 6].Text, out DateTime date) ? date : DateTime.MinValue;
                        string? contact = worksheet.Cells[row, 7].Text;
                        string? email = worksheet.Cells[row, 8].Text;

                        var familyMember = new FamilyMember
                        {
                            FamilyId = family.Id,
                            FirstName = firstName,
                            LastName = lastName,
                            DateOfBirth = dob,
                            Contact = contact,
                            Email = email
                        };
                        family.Members.Add(familyMember);
                        await _unitOfWork.Families.UpdateAsync(family);
                        await _unitOfWork.CompleteAsync();

                        // Read student details (if any)
                        string grade = worksheet.Cells[row, 9].Text ?? string.Empty;
                        if (!string.IsNullOrEmpty(grade))
                        {
                            int academicYear = int.TryParse(worksheet.Cells[row, 10].Text, out int year) ? year : DateTime.UtcNow.Year;
                            string? group = worksheet.Cells[row, 11].Text;

                            var student = new Student
                            {
                                FamilyMemberId = familyMember.Id,
                                Grade = grade,
                                AcademicYear = academicYear,
                                Group = group,
                                Status = "Active"
                            };
                            await _unitOfWork.Students.AddAsync(student);
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                    catch (Exception rowEx)
                    {
                        Console.WriteLine($"Error processing row {row}: {rowEx.Message}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing file: {ex.Message}");
                return false;
            }
        }
        public async Task<(bool Success, List<string> Errors)> ImportFamiliesFromExcelAsync(Stream excelStream)
        {
            var errors = new List<string>();
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            // Get all valid wards from the database
            var families = await _unitOfWork.Families.GetAllAsync();
            var validWards = families.Select(f => f.Ward).Distinct().ToList();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    string familyName = worksheet.Cells[row, 1].Text;
                    string ward = worksheet.Cells[row, 2].Text;
                    bool isRegistered = bool.Parse(worksheet.Cells[row, 3].Text);
                    string? churchRegistrationNumber = worksheet.Cells[row, 4].Text;
                    string? temporaryId = worksheet.Cells[row, 5].Text;

                    // Validate ward
                    if (!validWards.Contains(ward))
                    {
                        errors.Add($"Row {row}: Invalid ward '{ward}'. Ward does not exist in the system.");
                        continue;
                    }

                    // Validate ChurchRegistrationNumber or TemporaryId
                    if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                    {
                        errors.Add($"Row {row}: Church Registration Number is required for registered families.");
                        continue;
                    }
                    if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                    {
                        errors.Add($"Row {row}: Temporary ID is required for unregistered families.");
                        continue;
                    }

                    var family = new Family
                    {
                        FamilyName = familyName,
                        Ward = ward,
                        IsRegistered = isRegistered,
                        ChurchRegistrationNumber = isRegistered ? churchRegistrationNumber : null,
                        TemporaryID = !isRegistered ? temporaryId : null,
                        Status = "Active"
                    };

                    await _unitOfWork.Families.AddAsync(family);
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: Error processing family - {ex.Message}");
                }
            }

            if (!errors.Any())
            {
                await _unitOfWork.CompleteAsync();
                return (true, errors);
            }

            return (false, errors);
        }

        public async Task<(bool Success, List<string> Errors)> ImportStudentsFromExcelAsync(Stream excelStream)
        {
            var errors = new List<string>();
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            // Get all valid groups from the database
            var students = await _unitOfWork.Students.GetAllAsync();
            var validGroups = students.Where(s => !string.IsNullOrEmpty(s.Group)).Select(s => s.Group).Distinct().ToList();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    string firstName = worksheet.Cells[row, 1].Text;
                    string lastName = worksheet.Cells[row, 2].Text;
                    int familyMemberId = int.Parse(worksheet.Cells[row, 3].Text);
                    string grade = worksheet.Cells[row, 4].Text;
                    int academicYear = int.Parse(worksheet.Cells[row, 5].Text);
                    string group = worksheet.Cells[row, 6].Text;

                    // Validate family member
                    var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
                    if (familyMember == null)
                    {
                        errors.Add($"Row {row}: Family Member ID {familyMemberId} does not exist.");
                        continue;
                    }

                    // Validate group
                    if (!string.IsNullOrEmpty(group) && !validGroups.Contains(group))
                    {
                        errors.Add($"Row {row}: Invalid group '{group}'. Group does not exist in the system.");
                        continue;
                    }

                    var student = new Student
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        FamilyMemberId = familyMemberId,
                        Grade = grade,
                        AcademicYear = academicYear,
                        Group = group,
                        Status = "Active"
                    };

                    await _unitOfWork.Students.AddAsync(student);
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: Error processing student - {ex.Message}");
                }
            }

            if (!errors.Any())
            {
                await _unitOfWork.CompleteAsync();
                return (true, errors);
            }

            return (false, errors);
        }
    }
}