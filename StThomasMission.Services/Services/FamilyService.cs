using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class FamilyService : IFamilyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FamilyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Family> RegisterFamilyAsync(string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId)
        {
            if (string.IsNullOrEmpty(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));

            if (string.IsNullOrEmpty(ward))
                throw new ArgumentException("Ward is required.", nameof(ward));

            if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                throw new ArgumentException("Church registration number is required for registered families.", nameof(churchRegistrationNumber));

            if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(temporaryId));

            if (churchRegistrationNumber != null && !Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$"))
                throw new ArgumentException("Church registration number must be in format '10802XXXX'.", nameof(churchRegistrationNumber));

            if (temporaryId != null && !Regex.IsMatch(temporaryId, @"^TMP-\d{4}$"))
                throw new ArgumentException("Temporary ID must be in format 'TMP-XXXX'.", nameof(temporaryId));

            // Ensure ChurchRegistrationNumber is unique
            if (churchRegistrationNumber != null)
            {
                var existingFamily = (await _unitOfWork.Families.GetAllAsync())
                    .FirstOrDefault(f => f.ChurchRegistrationNumber == churchRegistrationNumber);
                if (existingFamily != null)
                    throw new InvalidOperationException("Church registration number already exists.");
            }

            var family = new Family
            {
                FamilyName = familyName,
                Ward = ward,
                IsRegistered = isRegistered,
                ChurchRegistrationNumber = churchRegistrationNumber,
                TemporaryID = temporaryId,
                Status = FamilyStatus.Active,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Families.AddAsync(family);
            await _unitOfWork.CompleteAsync();

            return family;
        }


        public async Task UpdateFamilyAsync(int familyId, string familyName, string ward, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, string status, string? migratedTo)
        {
            var family = await GetFamilyByIdAsync(familyId);

            if (string.IsNullOrEmpty(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));

            if (string.IsNullOrEmpty(ward))
                throw new ArgumentException("Ward is required.", nameof(ward));

            if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                throw new ArgumentException("Church registration number is required for registered families.", nameof(churchRegistrationNumber));

            if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(temporaryId));

            if (churchRegistrationNumber != null && !Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$"))
                throw new ArgumentException("Church registration number must be in format '10802XXXX'.", nameof(churchRegistrationNumber));

            if (temporaryId != null && !Regex.IsMatch(temporaryId, @"^TMP-\d{4}$"))
                throw new ArgumentException("Temporary ID must be in format 'TMP-XXXX'.", nameof(temporaryId));

            if (!Enum.TryParse<FamilyStatus>(status, true, out var parsedStatus))
                throw new ArgumentException("Invalid status.", nameof(status));

            if (churchRegistrationNumber != null && churchRegistrationNumber != family.ChurchRegistrationNumber)
            {
                var existingFamily = (await _unitOfWork.Families.GetAllAsync())
                    .FirstOrDefault(f => f.ChurchRegistrationNumber == churchRegistrationNumber && f.Id != familyId);

                if (existingFamily != null)
                    throw new InvalidOperationException("Church registration number already exists.");
            }

            family.FamilyName = familyName;
            family.Ward = ward;
            family.IsRegistered = isRegistered;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = temporaryId;
            family.Status = parsedStatus;
            family.MigratedTo = migratedTo;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();
        }


        public async Task ConvertTemporaryIdToChurchIdAsync(int familyId, string churchRegistrationNumber)
        {
            var family = await GetFamilyByIdAsync(familyId);
            if (family.IsRegistered) throw new InvalidOperationException("Family is already registered.");
            if (string.IsNullOrEmpty(churchRegistrationNumber)) throw new ArgumentException("Church registration number is required.", nameof(churchRegistrationNumber));
            if (!Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$")) throw new ArgumentException("Church registration number must be in format '10802XXXX'.", nameof(churchRegistrationNumber));

            // Ensure ChurchRegistrationNumber is unique
            var existingFamily = (await _unitOfWork.Families.GetAllAsync())
                .FirstOrDefault(f => f.ChurchRegistrationNumber == churchRegistrationNumber);
            if (existingFamily != null) throw new InvalidOperationException("Church registration number already exists.");

            family.IsRegistered = true;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = null;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Family> GetFamilyByIdAsync(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) throw new ArgumentException("Family not found.", nameof(familyId));
            return family;
        }

        public async Task<IEnumerable<Family>> GetFamiliesByWardAsync(string ward)
        {
            if (string.IsNullOrEmpty(ward))
                throw new ArgumentException("Ward is required.", nameof(ward));

            return await (_unitOfWork.Families as IFamilyRepository)!.GetByWardAsync(ward);
        }

        public async Task<IEnumerable<Family>> GetFamiliesByStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status))
                throw new ArgumentException("Status is required.", nameof(status));

            return await (_unitOfWork.Families as IFamilyRepository)!.GetByStatusAsync(status);
        }


        public async Task AddFamilyMemberAsync(int familyId, string firstName, string lastName, string? relation, DateTime dateOfBirth, string? contact, string? email, string? role)
        {
            var family = await GetFamilyByIdAsync(familyId);
            if (string.IsNullOrEmpty(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
            if (string.IsNullOrEmpty(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
            if (dateOfBirth > DateTime.UtcNow) throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
            if (contact != null && !Regex.IsMatch(contact, @"^\+?\d{10,15}$")) throw new ArgumentException("Invalid phone number.", nameof(contact));
            if (email != null && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new ArgumentException("Invalid email address.", nameof(email));

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
            if (string.IsNullOrEmpty(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
            if (string.IsNullOrEmpty(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
            if (dateOfBirth > DateTime.UtcNow) throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
            if (contact != null && !Regex.IsMatch(contact, @"^\+?\d{10,15}$")) throw new ArgumentException("Invalid phone number.", nameof(contact));
            if (email != null && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new ArgumentException("Invalid email address.", nameof(email));

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
            if (familyMember == null) throw new ArgumentException("Family member not found.", nameof(familyMemberId));
            return familyMember;
        }

        public async Task<IEnumerable<FamilyMember>> GetFamilyMembersByFamilyIdAsync(int familyId)
        {
            await GetFamilyByIdAsync(familyId); // Ensure family exists
            return await (_unitOfWork.FamilyMembers as IFamilyMemberRepository)!.GetByFamilyIdAsync(familyId);
        }


        public async Task<string> GeneratePrintDataAsync(int familyId)
        {
            var family = await GetFamilyByIdAsync(familyId);
            var members = await GetFamilyMembersByFamilyIdAsync(familyId);

            var sb = new StringBuilder();
            sb.AppendLine("Family Registration Details");
            sb.AppendLine("--------------------------");
            sb.AppendLine($"Family Name: {family.FamilyName}");
            sb.AppendLine($"Ward: {family.Ward}");
            sb.AppendLine($"Status: {family.Status}");

            if (family.IsRegistered)
            {
                sb.AppendLine($"Church Registration Number: {family.ChurchRegistrationNumber}");
            }
            else
            {
                sb.AppendLine($"Temporary ID: {family.TemporaryID}");
            }

            if (family.Status == FamilyStatus.Migrated)
            {
                sb.AppendLine($"Migrated To: {family.MigratedTo}");
            }

            sb.AppendLine($"Created Date: {family.CreatedDate:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("Family Members:");
            sb.AppendLine("---------------");

            foreach (var member in members)
            {
                sb.AppendLine($"- {member.FirstName} {member.LastName}, Relation: {member.Relation ?? "N/A"}, DOB: {member.DateOfBirth:yyyy-MM-dd}, Role: {member.Role ?? "N/A"}");
                sb.AppendLine($"  Contact: {member.Contact ?? "N/A"}, Email: {member.Email ?? "N/A"}");
            }

            return sb.ToString();
        }

    }
}