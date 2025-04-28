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

        public async Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, FamilyMemberRole relation, DateTime dateOfBirth, string? contact, string? email, string? role)
        {
            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));
            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));
            if (dateOfBirth > DateTime.UtcNow)
                throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
            if (!string.IsNullOrEmpty(contact) && !Regex.IsMatch(contact, @"^\+?\d{10,15}$"))
                throw new ArgumentException("Invalid phone number.", nameof(contact));
            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email address.", nameof(email));

            // Use IUnitOfWork directly instead of IFamilyService
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null)
                throw new ArgumentException("Family not found.", nameof(familyId));

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
                CreatedBy = "System",
            };

            await _unitOfWork.FamilyMembers.AddAsync(familyMember);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Create", nameof(FamilyMember), familyMember.Id.ToString(), $"Added family member: {familyMember.FirstName} {familyMember.LastName}");
        }

        public async Task UpdateFamilyMemberAsync(int familyMemberId, string firstName, string lastName, FamilyMemberRole relation, DateTime dateOfBirth, string? contact, string? email, string? role)
        {
            var familyMember = await GetFamilyMemberByIdAsync(familyMemberId);

            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));
            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));
            if (dateOfBirth > DateTime.UtcNow)
                throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
            if (!string.IsNullOrEmpty(contact) && !Regex.IsMatch(contact, @"^\+?\d{10,15}$"))
                throw new ArgumentException("Invalid phone number.", nameof(contact));
            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email address.", nameof(email));

            familyMember.FirstName = firstName;
            familyMember.LastName = lastName;
            familyMember.Relation = relation;
            familyMember.DateOfBirth = dateOfBirth;
            familyMember.Contact = contact;
            familyMember.Email = email;
            familyMember.Role = role;
            familyMember.UpdatedBy = "System";
           

            await _unitOfWork.FamilyMembers.UpdateAsync(familyMember);
            await _unitOfWork.CompleteAsync();

            await _auditService.LogActionAsync("System", "Update", nameof(FamilyMember), familyMemberId.ToString(), $"Updated family member: {familyMember.FirstName} {familyMember.LastName}");
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