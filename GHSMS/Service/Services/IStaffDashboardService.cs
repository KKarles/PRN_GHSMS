using Service.Models;

namespace Service.Services
{
    public interface IStaffDashboardService
    {
        Task<ResultModel> GetDashboardSummaryAsync();
    }
}