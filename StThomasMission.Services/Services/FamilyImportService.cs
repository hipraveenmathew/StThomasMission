using OfficeOpenXml;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class FamilyImportService
    {
        private readonly IFamilyService _familyService;
        private readonly IFamilyMemberService _familyMemberService;
        private readonly IUnitOfWork _unitOfWork;

        public FamilyImportService(IFamilyService familyService, IUnitOfWork unitOfWork, IFamilyMemberService familyMemberService)
        {
            _familyService = familyService;
            _unitOfWork = unitOfWork;
            ExcelPackage.License.SetNonCommercialPersonal("Praveen");
            _familyMemberService = familyMemberService;
        }

        public async Task ImportFamiliesFromExcelAsync(Stream fileStream)
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var rowData = ExtractRowData(worksheet, row);
                    if (!rowData.HasFather && !rowData.HasMother && !rowData.HasChildren)
                        continue;

                    int wardId = await GetOrCreateWardId(rowData.WardName);
                    string? churchRegistrationNumber = TransformRegistrationNumber(rowData.RegistrationNo);
                    bool isRegistered = !string.IsNullOrEmpty(churchRegistrationNumber);
                    string? temporaryId = isRegistered ? null : $"TMP-{row:D4}";
                    string familyName = rowData.FamilyName ?? rowData.FatherSurname ?? "Unknown Family";

                    var family = new Family
                    {
                        FamilyName = familyName,
                        WardId = wardId,
                        IsRegistered = isRegistered,
                        ChurchRegistrationNumber = churchRegistrationNumber,
                        TemporaryID = temporaryId,
                        Status = FamilyStatus.Active,
                        HouseNumber = rowData.HouseNo,
                        StreetName = rowData.StreetName,
                        City = rowData.City,
                        PostCode = rowData.PostCode,
                        Email = rowData.Email,
                        GiftAid = !string.IsNullOrEmpty(rowData.GiftAidStr) && rowData.GiftAidStr.Equals("Yes", StringComparison.OrdinalIgnoreCase),
                        ParishIndia = rowData.ParishIndia,
                        EparchyIndia = rowData.EparchyIndia,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow
                    };

                    await _familyService.RegisterFamilyAsync(family);
                    if (family.Id == 0)
                        continue;

                    await AddFamilyMemberFromExcelRow(family.Id, rowData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing row {row}: {ex.Message}");
                    continue;
                }
            }
        }

        private ExcelFamilyRow ExtractRowData(ExcelWorksheet worksheet, int row)
        {
            var rowData = new ExcelFamilyRow
            {
                RegistrationNo = worksheet.Cells[row, 1].Text?.Trim(),
                FamilyName = worksheet.Cells[row, 2].Text?.Trim(),
                WardName = worksheet.Cells[row, 3].Text?.Trim(),
                HouseNo = worksheet.Cells[row, 18].Text?.Trim(),
                StreetName = worksheet.Cells[row, 19].Text?.Trim(),
                City = worksheet.Cells[row, 20].Text?.Trim(),
                PostCode = worksheet.Cells[row, 21].Text?.Trim(),
                Email = worksheet.Cells[row, 22].Text?.Trim(),
                GiftAidStr = worksheet.Cells[row, 97].Text?.Trim(),
                ParishIndia = worksheet.Cells[row, 16].Text?.Trim(),
                EparchyIndia = worksheet.Cells[row, 17].Text?.Trim(),
                FatherFirstName = worksheet.Cells[row, 7].Text?.Trim(),
                FatherSurname = worksheet.Cells[row, 8].Text?.Trim(),
                FatherBaptismalName = worksheet.Cells[row, 9].Text?.Trim(),
                FatherContact = worksheet.Cells[row, 11].Text?.Trim(),
                FatherDob = worksheet.Cells[row, 12].Text?.Trim(),
                FatherBaptismDate = worksheet.Cells[row, 13].Text?.Trim(),
                FatherChrismationDate = worksheet.Cells[row, 14].Text?.Trim(),
                FatherHolyCommunionDate = worksheet.Cells[row, 15].Text?.Trim(),
                MarriageDate = worksheet.Cells[row, 34].Text?.Trim(),
                MotherFirstName = worksheet.Cells[row, 25].Text?.Trim(),
                MotherSurname = worksheet.Cells[row, 26].Text?.Trim(),
                MotherBaptismalName = worksheet.Cells[row, 27].Text?.Trim(),
                MotherContact = worksheet.Cells[row, 33].Text?.Trim(),
                MotherDob = worksheet.Cells[row, 30].Text?.Trim(),
                MotherBaptismDate = worksheet.Cells[row, 31].Text?.Trim(),
                MotherChrismationDate = worksheet.Cells[row, 32].Text?.Trim(),
                MotherHolyCommunionDate = worksheet.Cells[row, 28].Text?.Trim()
            };

            // Extract children
            for (int childIndex = 0; childIndex < 6; childIndex++)
            {
                int baseColumn = 35 + (childIndex * 9);
                if (!string.IsNullOrEmpty(worksheet.Cells[row, baseColumn + 1].Text))
                {
                    rowData.Children.Add(new ExcelChildData
                    {
                        FirstName = worksheet.Cells[row, baseColumn + 1].Text?.Trim(),
                        Surname = worksheet.Cells[row, baseColumn + 2].Text?.Trim(),
                        BaptismalName = worksheet.Cells[row, baseColumn + 3].Text?.Trim(),
                        Dob = worksheet.Cells[row, baseColumn + 5].Text?.Trim(),
                        BaptismDate = worksheet.Cells[row, baseColumn + 6].Text?.Trim(),
                        ChrismationDate = worksheet.Cells[row, baseColumn + 7].Text?.Trim(),
                        HolyCommunionDate = worksheet.Cells[row, baseColumn + 8].Text?.Trim()
                    });
                }
            }

            return rowData;
        }

        private async Task<int> GetOrCreateWardId(string? wardName)
        {
            if (string.IsNullOrEmpty(wardName))
                return 1; // Default "Temporary Ward"

            var ward = (await _unitOfWork.Wards.GetAsync(w => w.Name == wardName)).FirstOrDefault();
            if (ward == null)
            {
                ward = new Ward
                {
                    Name = wardName,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.Wards.AddAsync(ward);
                await _unitOfWork.CompleteAsync();
            }
            return ward.Id;
        }

        private string? TransformRegistrationNumber(string? registrationNo)
        {
            if (!string.IsNullOrEmpty(registrationNo) && registrationNo.StartsWith("10802") && registrationNo.Length == 8)
            {
                string suffix = registrationNo.Substring(5);
                return "108020" + suffix;
            }
            return null;
        }

        private async Task AddFamilyMemberFromExcelRow(int familyId, ExcelFamilyRow rowData)
        {
            

            // Add Father
            if (rowData.HasFather)
            {
                var father = new FamilyMember
                {
                    FamilyId = familyId,
                    FirstName = rowData.FatherFirstName ?? "Unknown",
                    LastName = rowData.FatherSurname ?? "Unknown",
                    BaptismalName = rowData.FatherBaptismalName,
                    Relation = FamilyMemberRole.Father,
                    DateOfBirth = DateTime.TryParse(rowData.FatherDob, out var fatherDob) ? fatherDob : DateTime.MinValue,
                    Contact = rowData.FatherContact,
                    Email = rowData.Email,
                    DateOfBaptism = DateTime.TryParse(rowData.FatherBaptismDate, out var fb) ? fb : null,
                    DateOfChrismation = DateTime.TryParse(rowData.FatherChrismationDate, out var fc) ? fc : null,
                    DateOfHolyCommunion = DateTime.TryParse(rowData.FatherHolyCommunionDate, out var fhc) ? fhc : null,
                    DateOfMarriage = DateTime.TryParse(rowData.MarriageDate, out var marriageDate) ? marriageDate : null,
                    CreatedBy = "System"
                };

                await _familyMemberService.AddFamilyMemberAsync(father);
               
            }

            // Add Mother
            if (rowData.HasMother)
            {
                var mother = new FamilyMember
                {
                    FamilyId = familyId,
                    FirstName = rowData.MotherFirstName ?? "Unknown",
                    LastName = rowData.MotherSurname ?? "Unknown",
                    BaptismalName = rowData.MotherBaptismalName,
                    Relation = FamilyMemberRole.Mother,
                    DateOfBirth = DateTime.TryParse(rowData.MotherDob, out var motherDob) ? motherDob : DateTime.MinValue,
                    Contact = rowData.MotherContact,
                    Email = rowData.Email,
                    DateOfBaptism = DateTime.TryParse(rowData.MotherBaptismDate, out var mb) ? mb : null,
                    DateOfChrismation = DateTime.TryParse(rowData.MotherChrismationDate, out var mc) ? mc : null,
                    DateOfHolyCommunion = DateTime.TryParse(rowData.MotherHolyCommunionDate, out var mhc) ? mhc : null,
                    DateOfMarriage = DateTime.TryParse(rowData.MarriageDate, out var mMarriageDate) ? mMarriageDate : null,
                    CreatedBy = "System"
                };

                await _familyMemberService.AddFamilyMemberAsync(mother);
            }

            // Add Children
            foreach (var child in rowData.Children)
            {
                var childMember = new FamilyMember
                {
                    FamilyId = familyId,
                    FirstName = child.FirstName ?? "Unknown",
                    LastName = child.Surname ?? "Unknown",
                    BaptismalName = child.BaptismalName,
                    Relation = FamilyMemberRole.Child,
                    DateOfBirth = DateTime.TryParse(child.Dob, out var cdob) ? cdob : DateTime.MinValue,
                    DateOfBaptism = DateTime.TryParse(child.BaptismDate, out var cb) ? cb : null,
                    DateOfChrismation = DateTime.TryParse(child.ChrismationDate, out var cc) ? cc : null,
                    DateOfHolyCommunion = DateTime.TryParse(child.HolyCommunionDate, out var chc) ? chc : null,
                    CreatedBy = "System"
                };
                await _familyMemberService.AddFamilyMemberAsync(childMember);
            }
        }
    }

   
}