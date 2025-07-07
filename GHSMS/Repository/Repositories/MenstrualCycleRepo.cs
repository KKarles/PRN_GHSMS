using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class MenstrualCycleRepo : GenericRepository<MenstrualCycle>, IMenstrualCycleRepo
    {
        public MenstrualCycleRepo(GenderHealthcareDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MenstrualCycle>> GetCyclesByUserAsync(int userId)
        {
            return await _dbSet
                .Where(mc => mc.UserId == userId)
                .OrderByDescending(mc => mc.StartDate)
                .ToListAsync();
        }

        public async Task<MenstrualCycle?> GetLatestCycleByUserAsync(int userId)
        {
            return await _dbSet
                .Where(mc => mc.UserId == userId)
                .OrderByDescending(mc => mc.StartDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MenstrualCycle>> GetActiveCyclesAsync()
        {
            return await _dbSet
                .Where(mc => mc.EndDate == null)
                .Include(mc => mc.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<MenstrualCycle>> GetCyclesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(mc => mc.UserId == userId && 
                            mc.StartDate >= DateOnly.FromDateTime(startDate) && 
                            mc.StartDate <= DateOnly.FromDateTime(endDate))
                .OrderByDescending(mc => mc.StartDate)
                .ToListAsync();
        }
    }
}