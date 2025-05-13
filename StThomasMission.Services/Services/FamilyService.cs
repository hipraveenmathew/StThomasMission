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

        public async Task<Family> RegisterFamilyAsync(Family family)
        {
            if (string.IsNullOrEmpty(family.FamilyName))
                throw new ArgumentException("Family name is required.", nameof(family.FamilyName));
            if (family.WardId <= 0)
                throw new ArgumentException("Ward ID must be a positive integer.", nameof(family.WardId));
            if (family.IsRegistered && string.IsNullOrEmpty(family.ChurchRegistrationNumber))
                throw new ArgumentException("Church Registration Number is required for registered families.", nameof(family.ChurchRegistrationNumber));
            if (!family.IsRegistered && string.IsNullOrEmpty(family.TemporaryID))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(family.TemporaryID));

            await _unitOfWork.Wards.GetByIdAsync(family.WardId);

            if (family.IsRegistered)
            {
                var existing = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(family.ChurchRegistrationNumber);
                if (existing != null)
                    throw new InvalidOperationException($"Church Registration Number {family.ChurchRegistrationNumber} already exists.");
            }
            else
            {
                var existing = await _unitOfWork.Families.GetByTemporaryIdAsync(family.TemporaryID);
                if (existing != null)
                    throw new InvalidOperationException($"Temporary ID {family.TemporaryID} already exists.");
            }

            family.Status = FamilyStatus.Active;
            family.CreatedBy = string.IsNullOrEmpty(family.CreatedBy) ? "System" : family.CreatedBy;
            family.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Families.AddAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(family.CreatedBy, "Create", nameof(Family), family.Id.ToString(), $"Registered family: {family.FamilyName}");
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

        public async Task UpdateFamilyAsync(Family family)
        {
            if (family.Id <= 0)
                throw new ArgumentException("Valid family ID is required.", nameof(family.Id));
            if (string.IsNullOrWhiteSpace(family.FamilyName))
                throw new ArgumentException("Family name is required.", nameof(family.FamilyName));
            if (family.WardId <= 0)
                throw new ArgumentException("Ward ID must be a positive integer.", nameof(family.WardId));
            if (family.IsRegistered && string.IsNullOrEmpty(family.ChurchRegistrationNumber))
                throw new ArgumentException("Church registration number is required for registered families.", nameof(family.ChurchRegistrationNumber));
            if (!family.IsRegistered && string.IsNullOrEmpty(family.TemporaryID))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(family.TemporaryID));

            var existingFamily = await GetFamilyByIdAsync(family.Id);
            await _unitOfWork.Wards.GetByIdAsync(family.WardId);

            if (!string.IsNullOrEmpty(family.ChurchRegistrationNumber) && family.ChurchRegistrationNumber != existingFamily.ChurchRegistrationNumber)
            {
                var existingByChurchReg = await _unitOfWork.Families.GetByChurchRegistrationNumberAsync(family.ChurchRegistrationNumber);
                if (existingByChurchReg != null && existingByChurchReg.Id != family.Id)
                    throw new ArgumentException("Church registration number already exists.", nameof(family.ChurchRegistrationNumber));
            }

            if (!string.IsNullOrEmpty(family.TemporaryID) && family.TemporaryID != existingFamily.TemporaryID)
            {
                var existingByTempId = await _unitOfWork.Families.GetByTemporaryIdAsync(family.TemporaryID);
                if (existingByTempId != null && existingByTempId.Id != family.Id)
                    throw new ArgumentException("Temporary ID already exists.", nameof(family.TemporaryID));
            }

            existingFamily.FamilyName = family.FamilyName;
            existingFamily.WardId = family.WardId;
            existingFamily.IsRegistered = family.IsRegistered;
            existingFamily.ChurchRegistrationNumber = family.ChurchRegistrationNumber;
            existingFamily.TemporaryID = family.TemporaryID;
            existingFamily.Status = family.Status;
            existingFamily.MigratedTo = family.Status == FamilyStatus.Migrated ? family.MigratedTo : null;
            existingFamily.UpdatedBy = string.IsNullOrEmpty(family.UpdatedBy) ? "System" : family.UpdatedBy;
            existingFamily.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Families.UpdateAsync(existingFamily);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(existingFamily.UpdatedBy, "Update", nameof(Family), existingFamily.Id.ToString(),
                $"Updated family: {existingFamily.FamilyName}, Status: {existingFamily.Status}");
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
        public async Task TransitionChildToNewFamilyAsync(int familyMemberId, string newFamilyName, int newWardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId)
        {
            // Get the family member (child)
            var child = await _unitOfWork.FamilyMembers.GetAsync(fm => fm.Id == familyMemberId);
            var childMember = child.FirstOrDefault() ?? throw new ArgumentException("Family member not found.", nameof(familyMemberId));

            // Get the original family
            var originalFamily = await _unitOfWork.Families.GetByIdAsync(childMember.FamilyId);
            if (originalFamily == null)
            {
                throw new ArgumentException("Original family not found.");
            }

            // Create a new family for the child
            var newFamily = new Family
            {
                FamilyName = newFamilyName,
                WardId = newWardId,
                IsRegistered = isRegistered,
                ChurchRegistrationNumber = churchRegistrationNumber,
                TemporaryID = temporaryId,
                Status = FamilyStatus.Active,
                PreviousFamilyId = originalFamily.Id, // Link to the original family
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Families.AddAsync(newFamily);
            await _unitOfWork.CompleteAsync();

            // Update the child's FamilyId to the new family
            childMember.FamilyId = newFamily.Id;
            await _unitOfWork.FamilyMembers.UpdateAsync(childMember);
            await _unitOfWork.CompleteAsync();

            // Log the transition
            await _auditService.LogActionAsync("System", "Transition", nameof(FamilyMember), childMember.Id.ToString(), $"Child {childMember.FullName} transitioned to new family: {newFamilyName}");
        }

        public async Task AddFamilyMemberAsync(FamilyMember familyMember)
        {
            if (familyMember.FamilyId <= 0)
                throw new ArgumentException("Valid family ID is required.", nameof(familyMember.FamilyId));
            if (string.IsNullOrWhiteSpace(familyMember.FirstName))
                throw new ArgumentException("First name is required.", nameof(familyMember.FirstName));
            if (string.IsNullOrWhiteSpace(familyMember.LastName))
                throw new ArgumentException("Last name is required.", nameof(familyMember.LastName));

            await GetFamilyByIdAsync(familyMember.FamilyId);

            familyMember.CreatedBy = string.IsNullOrEmpty(familyMember.CreatedBy) ? "System" : familyMember.CreatedBy;

            await _unitOfWork.FamilyMembers.AddAsync(familyMember);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(familyMember.CreatedBy, "Create", nameof(FamilyMember), familyMember.Id.ToString(),
                $"Added family member: {familyMember.FirstName} {familyMember.LastName} to family {familyMember.FamilyId}");
        }
        public async Task<FamilyMember> GetFamilyMemberByUserIdAsync(string userId)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetAsync(fm => fm.UserId == userId);
            return familyMember.FirstOrDefault() ?? throw new ArgumentException("Family member not found for the given user.", nameof(userId));
        }
       
    }
}