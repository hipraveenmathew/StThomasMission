using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
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

        public async Task AddFamilyMemberAsync(FamilyMember familyMember)
        {
            if (familyMember == null)
                throw new ArgumentNullException(nameof(familyMember));

            // Validate required fields
            if (string.IsNullOrWhiteSpace(familyMember.FirstName))
                throw new ArgumentException("First name is required.", nameof(familyMember.FirstName));
            if (string.IsNullOrWhiteSpace(familyMember.LastName))
                throw new ArgumentException("Last name is required.", nameof(familyMember.LastName));
            if (familyMember.DateOfBirth > DateTime.UtcNow)
                throw new ArgumentException("Date of birth cannot be in the future.", nameof(familyMember.DateOfBirth));

            // Validate contact
            if (!string.IsNullOrEmpty(familyMember.Contact) && !Regex.IsMatch(familyMember.Contact, @"^\+?\d{10,15}$"))
                throw new ArgumentException("Invalid phone number.", nameof(familyMember.Contact));

            // Validate email
            if (!string.IsNullOrEmpty(familyMember.Email) && !Regex.IsMatch(familyMember.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email address.", nameof(familyMember.Email));

            // Validate role length
            if (!string.IsNullOrEmpty(familyMember.Role) && familyMember.Role.Length > 50)
                throw new ArgumentException("Role cannot exceed 50 characters.", nameof(familyMember.Role));

            // Check family exists
            var family = await _unitOfWork.Families.GetByIdAsync(familyMember.FamilyId);
            if (family == null)
                throw new ArgumentException("Family not found.", nameof(familyMember.FamilyId));

            // Ensure CreatedBy is set
            familyMember.CreatedBy = string.IsNullOrWhiteSpace(familyMember.CreatedBy) ? "System" : familyMember.CreatedBy;

            await _unitOfWork.FamilyMembers.AddAsync(familyMember);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(
                familyMember.CreatedBy,
                "Create",
                nameof(FamilyMember),
                familyMember.Id.ToString(),
                $"Added family member: {familyMember.FullName}"
            );
        }


        public async Task UpdateFamilyMemberAsync(FamilyMember updatedMember)
        {
            if (updatedMember == null)
                throw new ArgumentNullException(nameof(updatedMember));

            if (string.IsNullOrWhiteSpace(updatedMember.FirstName))
                throw new ArgumentException("First name is required.", nameof(updatedMember.FirstName));
            if (string.IsNullOrWhiteSpace(updatedMember.LastName))
                throw new ArgumentException("Last name is required.", nameof(updatedMember.LastName));
            if (updatedMember.DateOfBirth > DateTime.UtcNow)
                throw new ArgumentException("Date of birth cannot be in the future.", nameof(updatedMember.DateOfBirth));
            if (!string.IsNullOrEmpty(updatedMember.Contact) && !Regex.IsMatch(updatedMember.Contact, @"^\+?\d{10,15}$"))
                throw new ArgumentException("Invalid phone number.", nameof(updatedMember.Contact));
            if (!string.IsNullOrEmpty(updatedMember.Email) && !Regex.IsMatch(updatedMember.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email address.", nameof(updatedMember.Email));

            var existingMember = await GetFamilyMemberByIdAsync(updatedMember.Id);
            if (existingMember == null)
                throw new ArgumentException("Family member not found.", nameof(updatedMember.Id));

            // Update fields
            existingMember.FirstName = updatedMember.FirstName;
            existingMember.LastName = updatedMember.LastName;
            existingMember.BaptismalName = updatedMember.BaptismalName;
            existingMember.Relation = updatedMember.Relation;
            existingMember.DateOfBirth = updatedMember.DateOfBirth;
            existingMember.DateOfDeath = updatedMember.DateOfDeath;
            existingMember.DateOfBaptism = updatedMember.DateOfBaptism;
            existingMember.DateOfChrismation = updatedMember.DateOfChrismation;
            existingMember.DateOfHolyCommunion = updatedMember.DateOfHolyCommunion;
            existingMember.DateOfMarriage = updatedMember.DateOfMarriage;
            existingMember.Contact = updatedMember.Contact;
            existingMember.Email = updatedMember.Email;
            existingMember.Role = updatedMember.Role;
            existingMember.UpdatedBy = updatedMember.UpdatedBy ?? "System";

            await _unitOfWork.FamilyMembers.UpdateAsync(existingMember);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync(
                existingMember.UpdatedBy,
                "Update",
                nameof(FamilyMember),
                existingMember.Id.ToString(),
                $"Updated family member: {existingMember.FirstName} {existingMember.LastName}");
        }


        public async Task DeleteFamilyMemberAsync(int familyMemberId)
        {
            var familyMember = await GetFamilyMemberByIdAsync(familyMemberId);

            var student = await _unitOfWork.Students.GetAsync(s => s.FamilyMemberId == familyMemberId);
            if (student.Any())
                throw new InvalidOperationException("Cannot delete family member with associated student.");

            await _unitOfWork.FamilyMembers.DeleteAsync(familyMemberId);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Delete", nameof(FamilyMember), familyMemberId.ToString(), $"Deleted family member: {familyMember.FirstName} {familyMember.LastName}");
        }

        public async Task<FamilyMember> GetFamilyMemberByIdAsync(int familyMemberId)
        {
            var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(familyMemberId);
            if (familyMember == null)
                throw new ArgumentException("Family member not found.", nameof(familyMemberId));
            return familyMember;
        }

        public async Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId)
        {
            // Use IUnitOfWork directly instead of IFamilyService
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null)
                throw new ArgumentException("Family not found.", nameof(familyId));

            return await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyId);
        }

        public async Task<FamilyMember> GetFamilyMemberByUserIdAsync(string userId)
        {
            var familyMembers = await _unitOfWork.FamilyMembers.GetAsync(fm => fm.UserId == userId);
            var familyMember = familyMembers.FirstOrDefault();
            if (familyMember == null)
                throw new ArgumentException("Family member not found for the given user.", nameof(userId));
            return familyMember;
        }
    }
}