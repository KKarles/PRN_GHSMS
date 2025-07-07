using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IDashboardService
    {
        Task<ResultModel> GetAdminDashboardStatsAsync();
        Task<ResultModel> GetManagerDashboardStatsAsync();
        Task<ResultModel> GetCustomerDashboardAsync(int customerId);
        Task<ResultModel> GetStaffDashboardStatsAsync();
    }
}