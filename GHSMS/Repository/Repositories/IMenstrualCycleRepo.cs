using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface IMenstrualCycleRepo : IGenericRepository<MenstrualCycle>
    {
        Task<IEnumerable<MenstrualCycle>> GetCyclesByUserAsync(int userId);
        Task<MenstrualCycle?> GetLatestCycleByUserAsync(int userId);
        Task<IEnumerable<MenstrualCycle>> GetActiveCyclesAsync();
        Task<IEnumerable<MenstrualCycle>> GetCyclesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    }
}