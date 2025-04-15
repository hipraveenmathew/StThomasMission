using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class ImportService : IImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyService _familyService;
        private readonly ICatechismService _catechismService;

        public ImportService(IUnitOfWork unitOfWork, IFamilyService familyService, ICatechismService catechismService)
        {
            _unitOfWork = unitOfWork;
            _familyService = familyService;
            _catechismService = catechismService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // EPPlus non-commercial license
        }

        public async Task ImportFamiliesAndStudentsAsync(Stream fileStream, string fileType)
        {
            if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            if (string.IsNullOrEmpty(fileType)) throw new ArgumentException("File type is required.", nameof(fileType));
            if (fileType != "Excel") throw new ArgumentException("Only Excel files are supported.", nameof(fileType));

            var errors = new List<string>();
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null || worksheet.Dimension == null)
            {
                throw new InvalidOperationException("Excel file is empty or invalid.");
            }

            var rowCount = worksheet.Dimension.Rows;
            if (rowCount < 2) throw new InvalidOperationException("Excel file contains no data rows.");

            // Begin transaction
            // Best Practice for EF Core Transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();


            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        // Read and validate family details
                        string familyName = worksheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrEmpty(familyName))
                        {
                            errors.Add($"Row {row}: Family name is required.");
                            continue;
                        }

                        string ward = worksheet.Cells[row, 2].Text?.Trim();
                        if (string.IsNullOrEmpty(ward))
                        {
                            errors.Add($"Row {row}: Ward is required.");
                            continue;
                        }

                        bool isRegistered = bool.TryParse(worksheet.Cells[row, 3].Text, out bool reg) && reg;
                        string? churchRegistrationNumber = isRegistered ? worksheet.Cells[row, 4].Text?.Trim() : null;
                        string? temporaryId = !isRegistered ? worksheet.Cells[row, 5].Text?.Trim() : null;

                        if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                        {
                            errors.Add($"Row {row}: Church Registration Number is required for registered families.");
                            continue;
                        }
                        if (isRegistered && !Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$"))
                        {
                            errors.Add($"Row {row}: Church Registration Number must be in format '10802XXXX'.");
                            continue;
                        }
                        if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                        {
                            errors.Add($"Row {row}: Temporary ID is required for unregistered families.");
                            continue;
                        }
                        if (!isRegistered && !Regex.IsMatch(temporaryId, @"^TMP-\d{4}$"))
                        {
                            errors.Add($"Row {row}: Temporary ID must be in format 'TMP-XXXX'.");
                            continue;
                        }

                        // Check if family already exists
                        Family family = null;
                        if (isRegistered)
                        {
                            family = (await _unitOfWork.Families.GetAllAsync())
                                .FirstOrDefault(f => f.ChurchRegistrationNumber == churchRegistrationNumber);
                        }
                        else
                        {
                            family = (await _unitOfWork.Families.GetAllAsync())
                                .FirstOrDefault(f => f.TemporaryID == temporaryId);
                        }

                        if (family == null)
                        {
                            family = await _familyService.RegisterFamilyAsync(
                                familyName,
                                ward,
                                isRegistered,
                                churchRegistrationNumber,
                                temporaryId
                            );
                        }

                        // Read and validate family member details
                        string firstName = worksheet.Cells[row, 6].Text?.Trim();
                        if (string.IsNullOrEmpty(firstName))
                        {
                            errors.Add($"Row {row}: First name is required.");
                            continue;
                        }

                        string lastName = worksheet.Cells[row, 7].Text?.Trim();
                        if (string.IsNullOrEmpty(lastName))
                        {
                            errors.Add($"Row {row}: Last name is required.");
                            continue;
                        }

                        if (!DateTime.TryParse(worksheet.Cells[row, 8].Text, out DateTime dob) || dob > DateTime.UtcNow)
                        {
                            errors.Add($"Row {row}: Invalid or future date of birth.");
                            continue;
                        }

                        string? contact = worksheet.Cells[row, 9].Text?.Trim();
                        if (!string.IsNullOrEmpty(contact) && !Regex.IsMatch(contact, @"^\+?\d{10,15}$"))
                        {
                            errors.Add($"Row {row}: Invalid phone number.");
                            continue;
                        }

                        string? email = worksheet.Cells[row, 10].Text?.Trim();
                        if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        {
                            errors.Add($"Row {row}: Invalid email address.");
                            continue;
                        }

                        string? role = worksheet.Cells[row, 11].Text?.Trim();
                        if (!string.IsNullOrEmpty(role) && role != "Parent")
                        {
                            errors.Add($"Row {row}: Role must be 'Parent' or empty.");
                            continue;
                        }

                        // Add family member
                        await _familyService.AddFamilyMemberAsync(
                            family.Id,
                            firstName,
                            lastName,
                            null, // Relation not specified in Excel
                            dob,
                            contact,
                            email,
                            role
                        );

                        // Get the newly added family member
                        var familyMember = (await _familyService.GetFamilyMembersByFamilyIdAsync(family.Id))
                            .FirstOrDefault(m => m.FirstName == firstName && m.LastName == lastName);
                        if (familyMember == null)
                        {
                            errors.Add($"Row {row}: Failed to add family member.");
                            continue;
                        }

                        // Read and validate student details
                        string grade = worksheet.Cells[row, 12].Text?.Trim();
                        if (!string.IsNullOrEmpty(grade))
                        {
                            if (!Regex.IsMatch(grade, @"^Year \d{1,2}$"))
                            {
                                errors.Add($"Row {row}: Grade must be in format 'Year X'.");
                                continue;
                            }

                            if (!int.TryParse(worksheet.Cells[row, 13].Text, out int academicYear) || academicYear < 2000 || academicYear > DateTime.UtcNow.Year)
                            {
                                errors.Add($"Row {row}: Invalid academic year.");
                                continue;
                            }

                            string? group = worksheet.Cells[row, 14].Text?.Trim();
                            if (!string.IsNullOrEmpty(group))
                            {
                                var existingGroups = (await _unitOfWork.Students.GetAllAsync())
                                    .Where(s => !string.IsNullOrEmpty(s.Group))
                                    .Select(s => s.Group)
                                    .Distinct()
                                    .ToList();
                                if (!existingGroups.Contains(group))
                                {
                                    errors.Add($"Row {row}: Group '{group}' does not exist in the system.");
                                    continue;
                                }
                            }

                            await _catechismService.AddStudentAsync(
                                familyMember.Id,
                                academicYear,
                                grade,
                                group,
                                null // StudentOrganisation not in Excel
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {row}: Error processing row - {ex.Message}");
                    }
                }

                if (errors.Any())
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Import failed with {errors.Count} errors:\n{string.Join("\n", errors)}");
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Import failed: {ex.Message}");
            }
        }
    }
}