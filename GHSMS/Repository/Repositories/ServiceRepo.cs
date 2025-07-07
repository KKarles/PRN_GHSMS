using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class ServiceRepo : GenericRepository<Service>, IServiceRepo
    {
        public ServiceRepo(GenderHealthcareDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Service>> GetAllWithAnalytesAsync()
        {
            return await _dbSet
                .Include(s => s.Analytes)
                .ToListAsync();
        }

        public async Task<Service?> GetByIdWithAnalytesAsync(int serviceId)
        {
            return await _dbSet
                .Include(s => s.Analytes)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);
        }

        public async Task<IEnumerable<Service>> GetByServiceTypeAsync(string serviceType)
        {
            return await _dbSet
                .Where(s => s.ServiceType == serviceType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetServicesByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Where(s => s.Price >= minPrice && s.Price <= maxPrice)
                .ToListAsync();
        }

        public async Task<Service?> GetByNameAsync(string serviceName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.ServiceName == serviceName);
        }
    }
}