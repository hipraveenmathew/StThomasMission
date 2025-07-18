using Microsoft.EntityFrameworkCore;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using StThomasMission.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StThomasMission.Infrastructure.Repositories
{
    public class GradeRepository : Repository<Grade>, IGradeRepository
    {
        public GradeRepository(StThomasMissionDbContext context) : base(context) { }

        public async Task<IEnumerable<GradeDto>> GetGradesInOrderAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderBy(g => g.Order)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Order = g.Order
                })
                .ToListAsync();
        }
    }
}