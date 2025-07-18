using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class FamilyMemberService : IFamilyMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        public FamilyMemberService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<FamilyMemberDto> GetFamilyMemberByIdAsync(int familyMemberId)
        {
            var member = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (member == null)
            {
                throw new NotFoundException(nameof(FamilyMember), familyMemberId);
            }

            // Map entity to DTO
            return new FamilyMemberDto
            {
                Id = member.Id,
                FamilyId = member.FamilyId,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Relation = member.Relation,
                DateOfBirth = member.DateOfBirth,
                Contact = member.Contact,
                Email = member.Email,
                BaptismalName = member.BaptismalName
            };
        }

        public async Task<IEnumerable<FamilyMemberDto>> GetMembersByFamilyIdAsync(int familyId)
        {
            return await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyId);
        }

        public async Task<FamilyMemberDto> AddMemberToFamilyAsync(CreateFamilyMemberRequest request, string userId)
        {
            // Ensure the family exists
            if (await _unitOfWork.Families.GetByIdAsync(request.FamilyId) == null)
            {
                throw new NotFoundException(nameof(Family), request.FamilyId);
            }

            var familyMember = new FamilyMember
            {
                FamilyId = request.FamilyId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Relation = request.Relation,
                DateOfBirth = request.DateOfBirth,
                Contact = request.Contact,
                Email = request.Email,
                BaptismalName = request.BaptismalName,
                CreatedBy = userId
            };

            var newMember = await _unitOfWork.FamilyMembers.AddAsync(familyMember);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Create", nameof(FamilyMember), newMember.Id.ToString(), $"Added member '{newMember.FullName}' to family ID {newMember.FamilyId}.");

            return await GetFamilyMemberByIdAsync(newMember.Id);
        }

        public async Task UpdateFamilyMemberAsync(int familyMemberId, UpdateFamilyMemberRequest request, string userId)
        {
            var member = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (member == null)
            {
                throw new NotFoundException(nameof(FamilyMember), familyMemberId);
            }

            member.FirstName = request.FirstName;
            member.LastName = request.LastName;
            member.Relation = request.Relation;
            member.DateOfBirth = request.DateOfBirth;
            member.Contact = request.Contact;
            member.Email = request.Email;
            member.BaptismalName = request.BaptismalName;
            member.UpdatedBy = userId;
            member.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.FamilyMembers.UpdateAsync(member);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Update", nameof(FamilyMember), member.Id.ToString(), $"Updated details for member '{member.FullName}'.");
        }

        public async Task DeleteFamilyMemberAsync(int familyMemberId, string userId)
        {
            var member = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (member == null)
            {
                throw new NotFoundException(nameof(FamilyMember), familyMemberId);
            }

            // Business Rule: Cannot delete a family member if they have an active student profile.
            var student = await _unitOfWork.Students.GetStudentByFamilyMemberId(familyMemberId);
            if (student != null)
            {
                throw new InvalidOperationException("Cannot delete a family member who has an associated student profile. Please migrate or delete the student first.");
            }

            member.IsDeleted = true;
            member.UpdatedBy = userId;
            member.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.FamilyMembers.UpdateAsync(member);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(userId, "Delete", nameof(FamilyMember), member.Id.ToString(), $"Soft-deleted member '{member.FullName}'.");
        }
    }
}