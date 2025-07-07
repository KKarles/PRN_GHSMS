using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface ICustomerProfileService
    {
        Task<ResultModel> GetCustomerProfileAsync(int customerId);
        Task<ResultModel> UpdateCustomerProfileAsync(int customerId, UpdateCustomerProfileDto updateDto);
        Task<ResultModel> GetNotificationSettingsAsync(int customerId);
        Task<ResultModel> UpdateNotificationSettingsAsync(int customerId, NotificationSettingsDto settingsDto);
        Task<ResultModel> GetCustomerDashboardAsync(int customerId);
        Task<ResultModel> DeleteCustomerAccountAsync(int customerId);
        Task<ResultModel> GetCustomerBookingHistoryAsync(int customerId);
        Task<ResultModel> GetCustomerTestResultsAsync(int customerId);
    }
}