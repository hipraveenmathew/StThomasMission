using iTextSharp.text.pdf;
using iTextSharp.text;
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

        public FamilyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Family> RegisterFamilyAsync(string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId)
        {
            if (string.IsNullOrEmpty(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));

            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null)
                throw new ArgumentException("Ward not found.", nameof(wardId));

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
                var existingFamily = (await _unitOfWork.Families.GetAllAsync())
                    .FirstOrDefault(f => f.ChurchRegistrationNumber == churchRegistrationNumber);
                if (existingFamily != null)
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
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Families.AddAsync(family);
            await _unitOfWork.CompleteAsync();

            return family;
        }

        public async Task UpdateFamilyAsync(int familyId, string familyName, int wardId, bool isRegistered, string? churchRegistrationNumber, string? temporaryId, FamilyStatus status, string? migratedTo)
        {
            var family = await GetFamilyByIdAsync(familyId);

            if (string.IsNullOrEmpty(familyName))
                throw new ArgumentException("Family name is required.", nameof(familyName));

            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null)
                throw new ArgumentException("Ward not found.", nameof(wardId));

            if (isRegistered && string.IsNullOrEmpty(churchRegistrationNumber))
                throw new ArgumentException("Church registration number is required for registered families.", nameof(churchRegistrationNumber));

            if (!isRegistered && string.IsNullOrEmpty(temporaryId))
                throw new ArgumentException("Temporary ID is required for unregistered families.", nameof(temporaryId));

            if (churchRegistrationNumber != null && !Regex.IsMatch(churchRegistrationNumber, @"^10802\d{4}$"))
                throw new ArgumentException("Church registration number must be in format '10802XXXX'.", nameof(churchRegistrationNumber));

            if (temporaryId != null && !Regex.IsMatch(temporaryId, @"^TMP-\d{4}$"))
                throw new ArgumentException("Temporary ID must be in format 'TMP-XXXX'.", nameof(temporaryId));

            if (churchRegistrationNumber != null && churchRegistrationNumber != family.ChurchRegistrationNumber)
            {
                var existingFamily = (await _unitOfWork.Families.GetAllAsync())
                    .FirstOrDefault(f => f.ChurchRegistrationNumber == churchRegistrationNumber && f.Id != familyId);
                if (existingFamily != null)
                    throw new InvalidOperationException("Church registration number already exists.");
            }

            family.FamilyName = familyName;
            family.WardId = wardId;
            family.IsRegistered = isRegistered;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = temporaryId;
            family.Status = status;
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

            var existingFamily = (await _unitOfWork.Families.GetAllAsync())
                .FirstOrDefault(f => f.ChurchRegistrationNumber == churchRegistrationNumber);
            if (existingFamily != null) throw new InvalidOperationException("Church registration number already exists.");

            family.IsRegistered = true;
            family.ChurchRegistrationNumber = churchRegistrationNumber;
            family.TemporaryID = null;

            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkFamilyAsDeletedAsync(int familyId)
        {
            var family = await GetFamilyByIdAsync(familyId);
            family.Status = FamilyStatus.Deleted;
            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkFamilyAsInactiveAsync(int familyId)
        {
            var family = await GetFamilyByIdAsync(familyId);
            family.Status = FamilyStatus.Inactive;
            await _unitOfWork.Families.UpdateAsync(family);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Family?> GetFamilyByIdAsync(int familyId)
        {
            var family = await _unitOfWork.Families.GetByIdAsync(familyId);
            if (family == null) throw new ArgumentException("Family not found.", nameof(familyId));
            return family;
        }

        public async Task<IEnumerable<Family>> GetFamiliesByWardAsync(int wardId)
        {
            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null)
                throw new ArgumentException("Ward not found.", nameof(wardId));

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

            using var memoryStream = new MemoryStream();
            var document = new Document();
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            document.Add(new Paragraph("Family Registration Slip"));
            document.Add(new Paragraph("--------------------------"));
            document.Add(new Paragraph($"Family Name: {family.FamilyName}"));
            document.Add(new Paragraph($"Ward ID: {family.WardId}"));
            document.Add(new Paragraph($"Status: {family.Status}"));

            if (family.IsRegistered)
                document.Add(new Paragraph($"Church Registration Number: {family.ChurchRegistrationNumber}"));
            else
                document.Add(new Paragraph($"Temporary ID: {family.TemporaryID}"));

            if (family.Status == FamilyStatus.Migrated)
                document.Add(new Paragraph($"Migrated To: {family.MigratedTo}"));

            document.Add(new Paragraph($"Created Date: {family.CreatedDate:yyyy-MM-dd}"));
            document.Add(new Paragraph("\nFamily Members:"));
            document.Add(new Paragraph("---------------"));

            foreach (var member in members)
            {
                document.Add(new Paragraph($"- {member.FirstName} {member.LastName}, Relation: {member.Relation ?? "N/A"}, DOB: {member.DateOfBirth:yyyy-MM-dd}, Role: {member.Role ?? "N/A"}"));
                document.Add(new Paragraph($"  Contact: {member.Contact ?? "N/A"}, Email: {member.Email ?? "N/A"}"));
            }

            document.Close();
            return memoryStream.ToArray();
        }
    }
}