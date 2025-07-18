using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public FamilyService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<FamilyDetailDto> RegisterFamilyAsync(RegisterFamilyRequest request, string userId)
        {
            if (await _unitOfWork.Wards.GetByIdAsync(request.WardId) == null)
            {
                throw new NotFoundException(nameof(Ward), request.WardId);
            }

            // Atomically get the next available temporary ID
            var tempId = await _unitOfWork.CountStorage.GetNextValueAsync("TemporaryID");

            var family = new Family
            {
                FamilyName = request.FamilyName,
                WardId = request.WardId,
                IsRegistered = false,
                TemporaryID = $"TMP-{tempId:D4}",
                Status = FamilyStatus.Active,
                CreatedBy = userId,
                HouseNumber = request.HouseNumber,
                StreetName = request.StreetName,
                City = request.City,
                PostCode = request.PostCode,
                Email = request.Email,
                GiftAid = request.GiftAid
            };

            var newFamily = await _unitOfWork.Families.AddAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Register", nameof(Family), newFamily.Id.ToString(), $"Registered new family '{newFamily.FamilyName}' with Temp ID {newFamily.TemporaryID}.");

            return (await _unitOfWork.Families.GetFamilyDetailByIdAsync(newFamily.Id))!;
        }
        // Add this method to your FamilyService class
        public async Task DeleteFamilyAsync(int familyId, string userId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) throw new NotFoundException(nameof(Family), familyId);

            // Business Rule: Cannot delete a family with active students.
            bool hasActiveStudents = await _unitOfWork.Students.AnyAsync(s => s.FamilyMember.FamilyId == familyId && s.Status == StudentStatus.Active);
            if (hasActiveStudents)
            {
                throw new InvalidOperationException("Cannot delete a family with active catechism students. Please migrate or deactivate the students first.");
            }

            family.IsDeleted = true;
            family.UpdatedBy = userId;
            family.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(Family), familyId.ToString(), $"Soft-deleted family '{family.FamilyName}'.");
        }
        // Add these two new methods to your FamilyService class
        public async Task<IPaginatedList<FamilySummaryDto>> SearchFamiliesPaginatedAsync(int pageNumber, int pageSize, string? searchTerm = null, int? wardId = null, bool? isRegistered = null)
        {
            // This is a clean pass-through to the repository's powerful search method
            return await _unitOfWork.Families.SearchFamiliesPaginatedAsync(pageNumber, pageSize, searchTerm, wardId, isRegistered);
        }

        public async Task<FamilyDetailDto> GetFamilyDetailByIdAsync(int familyId)
        {
            var familyDto = await _unitOfWork.Families.GetFamilyDetailByIdAsync(familyId);
            if (familyDto == null)
            {
                throw new NotFoundException(nameof(Family), familyId);
            }
            return familyDto;
        }

        public async Task UpdateFamilyDetailsAsync(int familyId, UpdateFamilyDetailsRequest request, string userId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) throw new NotFoundException(nameof(Family), familyId);

            if (family.WardId != request.WardId && await _unitOfWork.Wards.GetByIdAsync(request.WardId) == null)
            {
                throw new NotFoundException(nameof(Ward), request.WardId);
            }

            family.FamilyName = request.FamilyName;
            family.WardId = request.WardId;
            family.HouseNumber = request.HouseNumber;
            family.StreetName = request.StreetName;
            family.City = request.City;
            family.PostCode = request.PostCode;
            family.Email = request.Email;
            family.GiftAid = request.GiftAid;
            family.UpdatedBy = userId;
            family.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update Details", nameof(Family), familyId.ToString(), $"Updated details for family '{family.FamilyName}'.");
        }

        public async Task ConvertToRegisteredFamilyAsync(int familyId, string userId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) throw new NotFoundException(nameof(Family), familyId);
            if (family.IsRegistered) throw new InvalidOperationException("Family is already registered.");

            var regNum = await _unitOfWork.CountStorage.GetNextValueAsync("ChurchRegistrationNumber");

            family.IsRegistered = true;
            family.ChurchRegistrationNumber = regNum.ToString();
            family.TemporaryID = null; // Clear the temporary ID
            family.UpdatedBy = userId;
            family.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "ConvertToRegistered", nameof(Family), familyId.ToString(), $"Converted family '{family.FamilyName}' to registered with Church ID {family.ChurchRegistrationNumber}.");
        }

        public async Task MigrateFamilyAsync(int familyId, MigrateFamilyRequest request, string userId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) throw new NotFoundException(nameof(Family), familyId);

            family.Status = FamilyStatus.Migrated;
            family.MigratedTo = request.MigratedTo;
            family.UpdatedBy = userId;
            family.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Migrate", nameof(Family), familyId.ToString(), $"Migrated family '{family.FamilyName}' to {request.MigratedTo}.");
        }
    }
}