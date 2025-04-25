using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    /// <summary>
    /// Service for managing family members.
    /// </summary>
    public class FamilyMemberService : IFamilyMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFamilyService _familyService;

        public FamilyMemberService(IUnitOfWork unitOfWork, IFamilyService familyService)
        {
            _unitOfWork = unitOfWork;
            _familyService = familyService;
        }

        public async Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email, string? role)
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
            if (!string.IsNullOrEmpty(role) && role != "Parent" && role != "Child")
                throw new ArgumentException("Role must be 'Parent' or 'Child'.", nameof(role));

            await _familyService.GetFamilyByIdAsync(familyId); // Validates family exists

            var familyMember = new FamilyMember
            {
                FamilyId = familyId,
                FirstName = firstName,
                LastName = lastName,
                Relation = relation,
                DateOfBirth = dateOfBirth,
                Contact = contact,
                Email = email,
                Role = role
            };

            await _unitOfWork.FamilyMembers.AddAsync(familyMember);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateFamilyMemberAsync(int familyMemberId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email, string? role)
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
            if (!string.IsNullOrEmpty(role) && role != "Parent" && role != "Child")
                throw new ArgumentException("Role must be 'Parent' or 'Child'.", nameof(role));

            familyMember.FirstName = firstName;
            familyMember.LastName = lastName;
            familyMember.Relation = relation;
            familyMember.DateOfBirth = dateOfBirth;
            familyMember.Contact = contact;
            familyMember.Email = email;
            familyMember.Role = role;

            await _unitOfWork.FamilyMembers.UpdateAsync(familyMember);
            await _unitOfWork.CompleteAsync();
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
            await _familyService.GetFamilyByIdAsync(familyId); // Validates family exists
            return await _unitOfWork.FamilyMembers.GetByFamilyIdAsync(familyId);
        }
    }
}