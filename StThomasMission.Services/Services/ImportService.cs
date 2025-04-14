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
    }
}