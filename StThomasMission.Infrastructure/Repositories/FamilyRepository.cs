using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Enums;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using StThomasMission.Infrastructure.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class FamilyRepository : Repository<Family>, IFamilyRepository
    {
        public FamilyRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<FamilyDetailDto?> GetByChurchRegistrationNumberAsync(string churchRegistrationNumber)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(f => f.ChurchRegistrationNumber == churchRegistrationNumber)
                .Select(f => new FamilyDetailDto
                {
                    Id = f.Id,
                    FamilyName = f.FamilyName,
                    WardName = f.Ward.Name,
                    IsRegistered = f.IsRegistered,
                    ChurchRegistrationNumber = f.ChurchRegistrationNumber,
                    TemporaryID = f.TemporaryID,
                    Status = f.Status,
                    HouseNumber = f.HouseNumber,
                    StreetName = f.StreetName,
                    City = f.City,
                    PostCode = f.PostCode,
                    Email = f.Email,
                    Members = f.FamilyMembers.Select(m => new FamilyMemberDto
                    {
                        Id = m.Id,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Relation = m.Relation,
                        DateOfBirth = m.DateOfBirth,
                        Contact = m.Contact,
                        Email = m.Email
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
        // Add this new method to the FamilyRepository class

        public async Task<IEnumerable<FamilyRegistryDto>> GetAllFamilyRegistrationsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Select(f => new FamilyRegistryDto
                {
                    Id = f.Id,
                    ChurchRegistrationNumber = f.ChurchRegistrationNumber,
                    TemporaryID = f.TemporaryID
                })
                .ToListAsync();
        }

        public async Task<FamilyDetailDto?> GetFamilyDetailByIdAsync(int familyId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(f => f.Id == familyId)
                .Select(f => new FamilyDetailDto
                {
                    Id = f.Id,
                    FamilyName = f.FamilyName,
                    WardName = f.Ward.Name,
                    IsRegistered = f.IsRegistered,
                    ChurchRegistrationNumber = f.ChurchRegistrationNumber,
                    TemporaryID = f.TemporaryID,
                    Status = f.Status,
                    HouseNumber = f.HouseNumber,
                    StreetName = f.StreetName,
                    City = f.City,
                    PostCode = f.PostCode,
                    Email = f.Email,
                    GiftAid = f.GiftAid,
                    Members = f.FamilyMembers.Select(m => new FamilyMemberDto
                    {
                        Id = m.Id,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Relation = m.Relation,
                        DateOfBirth = m.DateOfBirth,
                        Contact = m.Contact,
                        Email = m.Email
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<RecipientContactInfo>> GetFamilyContactsByWardAsync(int wardId)
        {
            return await _context.FamilyMembers
                .AsNoTracking()
                .Where(fm => fm.Family.WardId == wardId && (!string.IsNullOrEmpty(fm.Email) || !string.IsNullOrEmpty(fm.Contact)))
                .Select(fm => new RecipientContactInfo
                {
                    FirstName = fm.FirstName,
                    Email = fm.Email,
                    PhoneNumber = fm.Contact
                })
                .Distinct() // Ensure we don't get the same contact info multiple times
                .ToListAsync();
        }

        public async Task<IEnumerable<FamilySummaryDto>> GetByWardAsync(int wardId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(f => f.WardId == wardId)
                .Select(f => new FamilySummaryDto
                {
                    Id = f.Id,
                    FamilyName = f.FamilyName,
                    WardName = f.Ward.Name,
                    IsRegistered = f.IsRegistered,
                    ChurchRegistrationNumber = f.ChurchRegistrationNumber,
                    TemporaryID = f.TemporaryID,
                    Status = f.Status
                })
                .OrderBy(f => f.FamilyName)
                .ToListAsync();
        }

        public async Task<IPaginatedList<FamilySummaryDto>> SearchFamiliesPaginatedAsync(int pageNumber, int pageSize, string? searchTerm = null, int? wardId = null, bool? isRegistered = null)
        {
            var query = _dbSet.AsNoTracking();

            if (wardId.HasValue)
            {
                query = query.Where(f => f.WardId == wardId.Value);
            }

            if (isRegistered.HasValue)
            {
                query = query.Where(f => f.IsRegistered == isRegistered.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(f => f.FamilyName.Contains(searchTerm)
                                      || (f.ChurchRegistrationNumber != null && f.ChurchRegistrationNumber.Contains(searchTerm))
                                      || (f.TemporaryID != null && f.TemporaryID.Contains(searchTerm)));
            }

            var dtoQuery = query.Select(f => new FamilySummaryDto
            {
                Id = f.Id,
                FamilyName = f.FamilyName,
                WardName = f.Ward.Name,
                IsRegistered = f.IsRegistered,
                ChurchRegistrationNumber = f.ChurchRegistrationNumber,
                TemporaryID = f.TemporaryID,
                Status = f.Status
            });

            return await PaginatedList<FamilySummaryDto>.CreateAsync(dtoQuery.OrderBy(f => f.FamilyName), pageNumber, pageSize);
        }
    }
}