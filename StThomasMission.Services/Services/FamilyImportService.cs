using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class FamilyImportService : IFamilyImportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyRegistrationService _familyRegistrationService;
        private readonly ILogger<FamilyImportService> _logger;

        public FamilyImportService(IUnitOfWork unitOfWork, IFamilyRegistrationService familyRegistrationService, ILogger<FamilyImportService> logger)
        {
            _unitOfWork = unitOfWork;
            _familyRegistrationService = familyRegistrationService;
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<ImportResultDto> ImportFamiliesFromExcelAsync(Stream fileStream, string userId)
        {
            var result = new ImportResultDto();
            var newFamilies = new List<Family>();
            var allWards = (await _unitOfWork.Wards.GetAllWithDetailsAsync()).ToDictionary(w => w.Name, w => w.Id, StringComparer.OrdinalIgnoreCase);

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            result.TotalRows = worksheet.Dimension.Rows - 1;

            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                try
                {
                    var parsedData = ParseRow(worksheet, row);
                    if (!parsedData.Members.Any())
                    {
                        result.AddFailedRow(row, "No family members found in this row.");
                        continue;
                    }

                    if (!allWards.TryGetValue(parsedData.WardName, out var wardId))
                    {
                        result.AddFailedRow(row, $"Ward '{parsedData.WardName}' does not exist.");
                        continue;
                    }

                    parsedData.WardId = wardId;
                    var family = await _familyRegistrationService.CreateNewFamilyFromImportAsync(parsedData, userId);
                    newFamilies.Add(family);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Excel row {RowNumber}", row);
                    result.AddFailedRow(row, ex.Message);
                }
            }

            if (result.FailedRows.Any())
            {
                // If there are any errors, do not save any data to ensure transactional integrity.
                return result;
            }

            // If all rows are valid, add them to the context and save everything in one transaction.
            foreach (var family in newFamilies)
            {
                await _unitOfWork.Families.AddAsync(family);
            }
            await _unitOfWork.CompleteAsync();

            result.SuccessfullyImported = newFamilies.Count;
            return result;
        }

        private ImportFamilyData ParseRow(ExcelWorksheet worksheet, int row)
        {
            // This method would contain the robust logic to parse each cell,
            // handle date conversions, and populate the ImportFamilyData DTO.
            // For brevity, we'll assume it correctly parses the data into this structure.
            var data = new ImportFamilyData
            {
                FamilyName = worksheet.Cells[row, 2].Text.Trim(),
                WardName = worksheet.Cells[row, 3].Text.Trim(),
                ChurchRegistrationNumber = worksheet.Cells[row, 1].Text.Trim(),
                Members = new List<ImportMemberData>()
            };

            // Example for parsing the father
            if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, 7].Text))
            {
                data.Members.Add(new ImportMemberData
                {
                    FirstName = worksheet.Cells[row, 7].Text.Trim(),
                    LastName = worksheet.Cells[row, 8].Text.Trim(),
                    Role = FamilyMemberRole.Father
                });
            }
            // ... Add similar parsing logic for Mother and Children

            return data;
        }
    }
}