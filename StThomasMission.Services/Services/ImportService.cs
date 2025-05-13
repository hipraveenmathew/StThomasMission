using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class ImportService : IImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyService _familyService;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IStudentService _studentService;
        private readonly IWardService _wardService;

        public ImportService(IUnitOfWork unitOfWork, IFamilyService familyService, IFamilyMemberService familyMemberService, IStudentService studentService, IWardService wardService)
        {
            _unitOfWork = unitOfWork;
            _familyService = familyService;
            _familyMemberService = familyMemberService;
            _studentService = studentService;
            _wardService = wardService;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task ImportFamiliesAndStudentsAsync(Stream fileStream, ImportType fileType)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
            if (fileType != ImportType.Excel)
                throw new ArgumentException("Only Excel files are supported.", nameof(fileType));

            var errors = new List<string>();
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null || worksheet.Dimension == null)
                throw new InvalidOperationException("Excel file is empty or invalid.");

            var rowCount = worksheet.Dimension.Rows;
            if (rowCount < 2)
                throw new InvalidOperationException("Excel file contains no data rows.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        string familyName = worksheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrEmpty(familyName))
                        {
                            errors.Add($"Row {row}: Family name is required.");
                            continue;
                        }

                        if (!int.TryParse(worksheet.Cells[row, 2].Text, out int wardId))
                        {
                            errors.Add($"Row {row}: Invalid ward ID.");
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

                        Family family = null;
                        if (isRegistered)
                        {
                            family = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
                        }
                        else
                        {
                            var families = await _unitOfWork.Families.GetAsync(f => f.TemporaryID == temporaryId && f.Status != FamilyStatus.Deleted);
                            family = families.FirstOrDefault();
                        }

                        if (family == null)
                        {
                            family = new Family
                            {
                                FamilyName = familyName,
                                WardId = wardId,
                                IsRegistered = isRegistered,
                                ChurchRegistrationNumber = churchRegistrationNumber,
                                TemporaryID = temporaryId,
                                Status = FamilyStatus.Active,
                                CreatedBy = "System",
                                CreatedDate = DateTime.UtcNow
                            };
                            await _familyService.RegisterFamilyAsync(family);
                        }

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
                        if (!string.IsNullOrEmpty(contact) && !Regex.IsMatch(contact, @"^+?\d{10,15}$"))
                        {
                            errors.Add($"Row {row}: Invalid phone number.");
                            continue;
                        }

                        string? email = worksheet.Cells[row, 10].Text?.Trim();
                        if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+.[^@\s]+$"))
                        {
                            errors.Add($"Row {row}: Invalid email address.");
                            continue;
                        }

                        string? role = worksheet.Cells[row, 11].Text?.Trim();
                        FamilyMemberRole relation = role switch
                        {
                            "Parent" => FamilyMemberRole.Parent,
                            "Child" => FamilyMemberRole.Child,
                            "Guardian" => FamilyMemberRole.Guardian,
                            _ => FamilyMemberRole.Other
                        };

                        var familyMember = new FamilyMember
                        {
                            FamilyId = family.Id,
                            FirstName = firstName,
                            LastName = lastName,
                            Relation = relation,
                            DateOfBirth = dob,
                            Contact = contact,
                            Email = email,
                            Role = role,
                            CreatedBy = "System"
                        };

                        await _familyMemberService.AddFamilyMemberAsync(familyMember);

                        var addedMember = (await _familyMemberService.GetFamilyMembersByFamilyIdAsync(family.Id))
                            .FirstOrDefault(m => m.FirstName == firstName && m.LastName == lastName);
                        if (addedMember == null)
                        {
                            errors.Add($"Row {row}: Failed to add family member.");
                             continue;
                        }

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

                            if (!int.TryParse(worksheet.Cells[row, 14].Text, out int groupId))
                            {
                                errors.Add($"Row {row}: Invalid group ID.");
                                continue;
                            }

                            await _studentService.EnrollStudentAsync(
                                addedMember.Id,
                                grade,
                                academicYear,
                                groupId,
                                null
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
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ImportWardsAsync(Stream fileStream, ImportType fileType)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));
            if (fileType != ImportType.Excel)
                throw new ArgumentException("Only Excel files are supported.", nameof(fileType));

            var errors = new List<string>();
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null || worksheet.Dimension == null)
                throw new InvalidOperationException("Excel file is empty or invalid.");

            var rowCount = worksheet.Dimension.Rows;
            if (rowCount < 2)
                throw new InvalidOperationException("Excel file contains no data rows.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        string name = worksheet.Cells[row, 1].Text?.Trim();
                        if (string.IsNullOrEmpty(name))
                        {
                            errors.Add($"Row {row}: Ward name is required.");
                            continue;
                        }

                        var existingWard = await _unitOfWork.Wards.GetByNameAsync(name);
                        if (existingWard != null)
                        {
                            errors.Add($"Row {row}: Ward '{name}' already exists.");
                            continue;
                        }

                        await _wardService.CreateWardAsync(name);
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
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}