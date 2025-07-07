using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IDashboardService
    {
        Task<ResultModel> GetDashboardStatsAsync();
        Task<ResultModel> GetRevenueStatsAsync();
        Task<ResultModel> GetUserStatsAsync();
        Task<ResultModel> GetBookingStatsAsync();
        Task<ResultModel> GetServiceStatsAsync();
        Task<ResultModel> GetMonthlyRevenueAsync(int year);
        Task<ResultModel> GetRevenueByServiceAsync();
        Task<ResultModel> GetRegistrationTrendsAsync(int days = 30);
        Task<ResultModel> GetPopularServicesAsync(int limit = 10);
    }
}