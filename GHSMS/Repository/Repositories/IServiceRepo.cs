using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface IServiceRepo : IGenericRepository<Service>
    {
        Task<IEnumerable<Service>> GetAllWithAnalytesAsync();
        Task<Service?> GetByIdWithAnalytesAsync(int serviceId);
        Task<IEnumerable<Service>> GetByServiceTypeAsync(string serviceType);
        Task<IEnumerable<Service>> GetServicesByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<Service?> GetByNameAsync(string serviceName);
    }
}