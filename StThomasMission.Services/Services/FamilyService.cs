using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IStudentService _studentService;

        public FamilyService(IUnitOfWork unitOfWork, IAuditService auditService,IStudentService studentService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _studentService = studentService;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<Family> RegisterFamilyAsync(string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId)
        {
            if (string.IsNullOrEmpty(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));
            if (wardId <= 0)
                throw new ArgumentException("Ward ID must be a positive integer.", nameof(wardId));
            if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                throw new ArgumentException("Church Registration Number is required for registered families.", nameof(churchRegistrationNumber));
            if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(temporaryId));

            await _unitOfWork.Wards.GetByIdAsync(wardId);

            if (isRegistered)
            {
                var existing = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
                if (existing != null)
                    throw new InvalidOperationException($"Church Registration Number {churchRegistrationNumber} already exists.");
            }
            else
            {
                var existing = await _unitOfWork.Families.GetByTemporaryIdAsync(temporaryId);
                if (existing != null)
                    throw new InvalidOperationException($"Temporary ID {temporaryId} already exists.");
            }

            var family = new Family
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

            await _unitOfWork.Families.AddAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(Family), family.Id.ToString(), $"Registered family: {familyName}");
            return family;
        }

        public async Task<Family> GetFamilyByIdAsync(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null || family.Status == FamilyStatus.Deleted)
                throw new ArgumentException("Family not found.", nameof(familyId));
            return family;
        }

        public async Task<Family?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber)
        {
            return await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
        }

        public async Task<Family?> GetByTemporaryIdAsync(string temporaryId)
        {
            return await _unitOfWork.Families.GetByTemporaryIdAsync(temporaryId);
        }

        public async Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId)
        {
            return await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyId);
        }

        public async Task EnrollStudentAsync(int familyMemberId, string grade, int academicYear, int groupId, string? studentOrganisation)
        {
            await _studentService.EnrollStudentAsync(familyMemberId, grade, academicYear, groupId, studentOrganisation); // Use IStudentService
        }

        // ... (other methods remain the same)

        public async Task UpdateFamilyAsync(int familyId, string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, FamilyStatus status, string? migratedTo)
        {
            var family = await GetFamilyByIdAsync(familyId);

            if (string.IsNullOrWhiteSpace(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));

            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null)
                throw new ArgumentException("Ward not found.", nameof(wardId));

            if (isRegistered && string.IsNullOrWhiteSpace(churchRegistrationNumber))
                throw new ArgumentException("Church registration number is required for registered families.", nameof(churchRegistrationNumber));
            if (!isRegistered && string.IsNullOrWhiteSpace(temporaryId))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(temporaryId));

            if (churchRegistrationNumber != null && churchRegistrationNumber != family.ChurchRegistrationNumber)
            {
                var existingByChurchReg = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
                if (existingByChurchReg != null && existingByChurchReg.Id != familyId)
                    throw new ArgumentException("Church registration number already exists.", nameof(churchRegistrationNumber));
            }

            if (temporaryId != null && temporaryId != family.TemporaryID)
            {
                var existingByTempId = await _unitOfWork.Families.GetByTemporaryIdAsync(temporaryId);
                if (existingByTempId != null && existingByTempId.Id != familyId)
                    throw new ArgumentException("Temporary ID already exists.", nameof(temporaryId));
            }

            family.FamilyName = familyName;
            family.WardId = wardId;
            family.IsRegistered = isRegistered;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = temporaryId;
            family.Status = status;
            family.MigratedTo = status == FamilyStatus.Migrated ? migratedTo : null;
            family.UpdatedBy = "System";
            family.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Family), familyId.ToString(), $"Updated family: {familyName}, Status: {status}");
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
                            column.Item().Text($"Registration Slip: {family.FamilyName}").FontSize(16).Bold();
                            column.Item().Text($"Ward: {family.Ward.Name} | Status: {family.Status}");
                            column.Item().Text($"Registration: {(family.IsRegistered ? family.ChurchRegistrationNumber : family.TemporaryID)}");
                            if (family.Status == FamilyStatus.Migrated)
                                column.Item().Text($"Migrated To: {family.MigratedTo}");
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            column.Item().PaddingTop(10).Text("Family Members").FontSize(14).Bold();
                            foreach (var member in members)
                            {
                                column.Item().Text($"{member.FirstName} {member.LastName} - {member.Relation} - DOB: {member.DateOfBirth:yyyy-MM-dd}");
                            }
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        public async Task<string> NewChurchIdAsync()
        {
            // Get all families where ChurchRegistrationNumber is not null
            var families = await _unitOfWork.Families.GetAllQueryable()
                .Where(f => f.ChurchRegistrationNumber != null)
                .Select(f => f.ChurchRegistrationNumber)
                .ToListAsync();

            // If no registered families exist, start with a default ChurchRegistrationNumber
            if (!families.Any())
            {
                return "108020001"; // Starting point for new registrations
            }

            // Parse the ChurchRegistrationNumber values as integers and find the maximum
            var maxChurchId = families
                .Select(num => int.Parse(num))
                .Max();

            // Increment the maximum value by 1 and format it back to a string
            var newChurchId = (maxChurchId + 1).ToString(); // Ensures at least 8 digits with leading zeros if needed

            return newChurchId;
        }
        public async Task ConvertTemporaryIdToChurchIdAsync(int familyId)
        {
            var family = await GetFamilyByIdAsync(familyId);
            if (family.IsRegistered)
                throw new InvalidOperationException("Family is already registered.");

            var churchRegistrationNumber = await NewChurchIdAsync();
            //var existing = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(churchRegistrationNumber);
            //if (existing != null && existing.Id != familyId)
            //    throw new InvalidOperationException($"Church Registration Number {churchRegistrationNumber} already exists.");

            family.IsRegistered = true;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = null;
            family.UpdatedDate = DateTime.UtcNow;
            family.UpdatedBy = "System";

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(Family), familyId.ToString(), $"Converted family to registered: {churchRegistrationNumber}");
        }

        public IQueryable<Family> GetFamiliesQueryable(string? searchString = null, string? ward = null, FamilyStatus? status = null)
        {
            var query = _unitOfWork.Families.GetAllQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                var searchLower = searchString.ToLower();
                query = query.Where(f =>
                    f.FamilyName.ToLower().Contains(searchLower) ||
                    f.ChurchRegistrationNumber.ToLower().Contains(searchLower) ||
                    f.TemporaryID.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrEmpty(ward) && int.TryParse(ward, out int wardId))
            {
                query = query.Where(f => f.WardId == wardId);
            }

            if (status.HasValue)
            {
                query = query.Where(f => f.Status == status.Value);
            }

            return query.Include(f => f.Ward);
        }

        public async Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, FamilyMemberRole relation, DateTime dateOfBirth, string? contact, string? email, string? role)
        {
            await GetFamilyByIdAsync(familyId);

            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));

            var familyMember = new FamilyMember
            {
                FamilyId = familyId,
                FirstName = firstName,
                LastName = lastName,
                Relation = relation,
                DateOfBirth = dateOfBirth,
                Contact = contact,
                Email = email,
                Role = role,
                CreatedBy = "System"               
            };

            await _unitOfWork.FamilyMembers.AddAsync(familyMember);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(FamilyMember), familyMember.Id.ToString(), $"Added family member: {firstName} {lastName} to family {familyId}");
        }
        public async Task<FamilyMember> GetFamilyMemberByUserIdAsync(string userId)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetAsync(fm => fm.UserId == userId);
            return familyMember.FirstOrDefault() ?? throw new ArgumentException("Family member not found for the given user.", nameof(userId));
        }
    }
}