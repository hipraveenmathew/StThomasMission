using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    /// <summary>
    /// Service for managing wards.
    /// </summary>
    public class WardService : IWardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddWardAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Ward name is required.", nameof(name));

            var existingWard = await _unitOfWork.Wards.GetByNameAsync(name);
            if (existingWard != null)
                throw new InvalidOperationException($"Ward '{name}' already exists.");

            var ward = new Ward
            {
                Name = name,
                IsActive = true
            };

            await _unitOfWork.Wards.AddAsync(ward);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateWardAsync(int wardId, string name, bool isActive)
        {
            var ward = await GetWardByIdAsync(wardId);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Ward name is required.", nameof(name));

            var existingWard = await _unitOfWork.Wards.GetByNameAsync(name);
            if (existingWard != null && existingWard.Id != wardId)
                throw new InvalidOperationException($"Ward '{name}' already exists.");

            ward.Name = name;
            ward.IsActive = isActive;

            await _unitOfWork.Wards.UpdateAsync(ward);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteWardAsync(int wardId)
        {
            var ward = await GetWardByIdAsync(wardId);

            var families = await _unitOfWork.Families.GetByWardAsync(wardId);
            if (families.Any())
                throw new InvalidOperationException("Cannot delete ward with associated families.");

            await _unitOfWork.Wards.DeleteAsync(ward);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<Ward> GetWardByIdAsync(int wardId)
        {
            var ward = await _unitOfWork.Wards.GetByIdAsync(wardId);
            if (ward == null)
                throw new ArgumentException("Ward not found.", nameof(wardId));
            return ward;
        }

        public async Task<IEnumerable<Ward>> GetAllWardsAsync()
        {
            return await _unitOfWork.Wards.GetAllAsync();
        }
    }
}