using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IServiceCatalogService
    {
        Task<ResultModel> GetAllServicesAsync();
        Task<ResultModel> GetServiceByIdAsync(int serviceId);
        Task<ResultModel> GetServicesByTypeAsync(string serviceType);
        Task<ResultModel> GetServicesByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<ResultModel> CreateServiceAsync(CreateServiceDto createServiceDto);
        Task<ResultModel> UpdateServiceAsync(int serviceId, UpdateServiceDto updateServiceDto);
        Task<ResultModel> DeleteServiceAsync(int serviceId);
        Task<ResultModel> GetAllAnalytesAsync();
        Task<ResultModel> GetServiceWithAnalytesAsync(int serviceId);
    }
}