using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class ImportService : IImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImportService> _logger;

        public ImportService(IUnitOfWork unitOfWork, ILogger<ImportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<ImportResultDto> ImportDataAsync(Stream fileStream, string userId)
        {
            var result = new ImportResultDto();
            var newFamilies = new List<Family>();

            // 1. Pre-fetch existing data into memory once to avoid N+1 queries.
            var allWards = (await _unitOfWork.Wards.GetAllWithDetailsAsync())
                .ToDictionary(w => w.Name, w => w.Id, System.StringComparer.OrdinalIgnoreCase);

            // Corrected: Use the efficient repository method that returns a lightweight DTO.
            var allExistingRegistrations = await _unitOfWork.Families.GetAllFamilyRegistrationsAsync();
            var familiesByRegNo = allExistingRegistrations
                .Where(f => !string.IsNullOrEmpty(f.ChurchRegistrationNumber))
                .ToDictionary(f => f.ChurchRegistrationNumber!, f => f.Id);

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) throw new System.InvalidOperationException("Excel file is empty or invalid.");

            result.TotalRows = worksheet.Dimension.Rows - 1;

            // 2. Parse and validate all rows in memory before touching the database.
            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                try
                {
                    string familyName = worksheet.Cells[row, 1].Text?.Trim();
                    string wardName = worksheet.Cells[row, 2].Text?.Trim();
                    string regNo = worksheet.Cells[row, 3].Text?.Trim();
                    string memberName = worksheet.Cells[row, 4].Text?.Trim();

                    // Basic validation
                    if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(wardName) || string.IsNullOrEmpty(memberName))
                    {
                        result.AddFailedRow(row, "FamilyName, WardName, and MemberName are required.");
                        continue;
                    }
                    if (!allWards.TryGetValue(wardName, out var wardId))
                    {
                        result.AddFailedRow(row, $"Ward '{wardName}' does not exist.");
                        continue;
                    }

                    // Find or create the family entity in our in-memory list
                    var family = newFamilies.FirstOrDefault(f => f.FamilyName == familyName && f.WardId == wardId);
                    if (family == null)
                    {
                        if (!string.IsNullOrEmpty(regNo) && familiesByRegNo.ContainsKey(regNo))
                        {
                            result.AddFailedRow(row, $"Family with registration number '{regNo}' already exists.");
                            continue;
                        }

                        family = new Family
                        {
                            FamilyName = familyName,
                            WardId = wardId,
                            IsRegistered = !string.IsNullOrEmpty(regNo),
                            ChurchRegistrationNumber = string.IsNullOrEmpty(regNo) ? null : regNo,
                            TemporaryID = string.IsNullOrEmpty(regNo) ? $"TMP-IMPORT-{newFamilies.Count + 1:D4}" : null,
                            CreatedBy = userId
                        };
                        newFamilies.Add(family);
                    }

                    // Add the family member to the in-memory family object
                    family.FamilyMembers.Add(new FamilyMember
                    {
                        FirstName = memberName,
                        LastName = familyName,
                        Relation = Core.Enums.FamilyMemberRole.Other,
                        DateOfBirth = new System.DateTime(2000, 1, 1),
                        CreatedBy = userId
                    });
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error processing Excel row {RowNumber}", row);
                    result.AddFailedRow(row, ex.Message);
                }
            }

            // 3. If any rows failed validation, abort the entire transaction.
            if (result.FailedRows.Any())
            {
                return result;
            }

            // 4. If all rows are valid, save everything in a single transaction.
            if (newFamilies.Any())
            {
                foreach (var family in newFamilies)
                {
                    await _unitOfWork.Families.AddAsync(family);
                }
                result.SuccessfullyImported = await _unitOfWork.CompleteAsync();
            }

            return result;
        }
    }
}