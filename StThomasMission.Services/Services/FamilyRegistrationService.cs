using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Services.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Services.Services
{
    public class FamilyRegistrationService : IFamilyRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FamilyRegistrationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Family> CreateNewFamilyFromImportAsync(ImportFamilyData data, string userId)
        {
            bool isRegistered = !string.IsNullOrEmpty(data.ChurchRegistrationNumber);
            string? temporaryId = isRegistered ? null : $"TMP-{await _unitOfWork.CountStorage.GetNextValueAsync("TemporaryID"):D4}";

            var family = new Family
            {
                FamilyName = data.FamilyName,
                WardId = data.WardId,
                IsRegistered = isRegistered,
                ChurchRegistrationNumber = data.ChurchRegistrationNumber,
                TemporaryID = temporaryId,
                Status = FamilyStatus.Active,
                CreatedBy = userId
            };

            foreach (var memberData in data.Members)
            {
                family.FamilyMembers.Add(new FamilyMember
                {
                    FirstName = memberData.FirstName,
                    LastName = memberData.LastName,
                    Relation = memberData.Role,
                    CreatedBy = userId
                    // ... map other member properties
                });
            }
            return family;
        }
        // ... Other family registration methods would go here
    }
}