using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public FamilyService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<Family> RegisterFamilyAsync(string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId)
        {
            if (string.IsNullOrEmpty(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));

            await _unitOfWork.Wards.GetByIdAsync(wardId);

            if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                throw new ArgumentException("Church registration number is required for registered families.", nameof(churchRegistrationNumber));
            if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(temporaryId));
            if (churchRegistrationNumber != null && !Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$"))
                throw new ArgumentException("Church registration number must be in format '10802XXXX'.", nameof(churchRegistrationNumber));
            if (temporaryId != null && !Regex.IsMatch(temporaryId, @"^TMP-\d{4}$"))
                throw new ArgumentException("Temporary ID must be in format 'TMP-XXXX'.", nameof(temporaryId));

            if (churchRegistrationNumber != null)
            {
                var existing = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
                if (existing != null)
                    throw new InvalidOperationException("Church registration number already exists.");
            }

            var family = new Family
            {
                FamilyName = familyName,
                WardId = wardId,
                IsRegistered = isRegistered,
                ChurchRegistrationNumber = churchRegistrationNumber,
                TemporaryID = temporaryId,
                Status = FamilyStatus.Active,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };

            await _unitOfWork.Families.AddAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Family), family.Id.ToString(), $"Registered family: {familyName}");

            return family;
        }

        public async Task UpdateFamilyAsync(int familyId, string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, FamilyStatus status, string? migratedTo)
        {
            var family = await GetFamilyByIdAsync(familyId);

            if (string.IsNullOrEmpty(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));

            await _unitOfWork.Wards.GetByIdAsync(wardId);

            if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                throw new ArgumentException("Church registration number is required for registered families.", nameof(churchRegistrationNumber));
            if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(temporaryId));
            if (churchRegistrationNumber != null && !Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$"))
                throw new ArgumentException("Church registration number must be in format '10802XXXX'.", nameof(churchRegistrationNumber));
            if (temporaryId != null && !Regex.IsMatch(temporaryId, @"^TMP-\d{4}$"))
                throw new ArgumentException("Temporary ID must be in format 'TMP-XXXX'.", nameof(temporaryId));
            if (status == FamilyStatus.Migrated && string.IsNullOrEmpty(migratedTo))
                throw new ArgumentException("MigratedTo is required for migrated status.", nameof(migratedTo));

            if (churchRegistrationNumber != null && churchRegistrationNumber != family.ChurchRegistrationNumber)
            {
                var existing = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
                if (existing != null && existing.Id != familyId)
                    throw new InvalidOperationException("Church registration number already exists.");
            }

            family.FamilyName = familyName;
            family.WardId = wardId;
            family.IsRegistered = isRegistered;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = temporaryId;
            family.Status = status;
            family.MigratedTo = status == FamilyStatus.Migrated ? migratedTo : null;
            family.UpdatedDate = DateTime.UtcNow;
            family.UpdatedBy = "System";

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Family), familyId.ToString(), $"Updated family: {familyName}, Status: {status}");
        }

        public async Task ConvertTemporaryIdToChurchIdAsync(int familyId, string churchRegistrationNumber)
        {
            var family = await GetFamilyByIdAsync(familyId);

            if (family.IsRegistered)
                throw new InvalidOperationException("Family is already registered.");
            if (string.IsNullOrEmpty(churchRegistrationNumber))
                throw new ArgumentException("Church registration number is required.", nameof(churchRegistrationNumber));
            if (!Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$"))
                throw new ArgumentException("Church registration number must be in format '10802XXXX'.", nameof(churchRegistrationNumber));

            var existing = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
            if (existing != null)
                throw new InvalidOperationException("Church registration number already exists.");

            family.IsRegistered = true;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = null;
            family.UpdatedDate = DateTime.UtcNow;
            family.UpdatedBy = "System";

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Family), familyId.ToString(), $"Converted family to Church ID: {churchRegistrationNumber}");
        }

        public async Task DeleteFamilyAsync(int familyId)
        {
            var family = await GetFamilyByIdAsync(familyId);
            var members = await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyId);
            if (members.Any(m => m.StudentProfile != null))
                throw new InvalidOperationException("Cannot delete family with enrolled students.");

            family.Status = FamilyStatus.Deleted;
            family.UpdatedDate = DateTime.UtcNow;
            family.UpdatedBy = "System";

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(Family), familyId.ToString(), $"Soft-deleted family: {family.FamilyName}");
        }

        public async Task<Family?> GetFamilyByIdAsync(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null || family.Status == FamilyStatus.Deleted)
                throw new ArgumentException("Family not found.", nameof(familyId));
            return family;
        }

        public async Task<IEnumerable<Family>> GetFamiliesByWardAsync(int wardId)
        {
            await _unitOfWork.Wards.GetByIdAsync(wardId);
            return await _unitOfWork.Families.GetByWardAsync(wardId);
        }

        public async Task<IEnumerable<Family>> GetFamiliesByStatusAsync(FamilyStatus status)
        {
            return await _unitOfWork.Families.GetByStatusAsync(status);
        }

        public async Task<byte[]> GenerateRegistrationSlipAsync(int familyId)
        {
            var family = await GetFamilyByIdAsync(familyId);
            var members = await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyId);

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
                            column.Item().Text("Family Registration Slip").FontSize(16).Bold();
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            column.Item().PaddingVertical(5).Text(text =>
                            {
                                text.Span("Family Name: ").Bold();
                                text.Span(family.FamilyName);
                                text.Line($"Ward ID: {family.WardId}");
                                text.Line($"Status: {family.Status}");
                                text.Line(family.IsRegistered
                                    ? $"Church Registration Number: {family.ChurchRegistrationNumber}"
                                    : $"Temporary ID: {family.TemporaryID}");
                                if (family.Status == FamilyStatus.Migrated)
                                    text.Line($"Migrated To: {family.MigratedTo}");
                                text.Line($"Created Date: {family.CreatedDate:yyyy-MM-dd}");
                            });

                            column.Item().PaddingTop(10).Text("Family Members").FontSize(14).Bold();
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            foreach (var member in members)
                            {
                                column.Item().PaddingVertical(5).Text(text =>
                                {
                                    text.Span($"{member.FullName}, Relation: {member.Relation}, DOB: {member.DateOfBirth:yyyy-MM-dd}, Role: {member.Role ?? "N/A"}");
                                    text.Line($"Contact: {member.Contact ?? "N/A"}, Email: {member.Email ?? "N/A"}");
                                });
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }
    }
}