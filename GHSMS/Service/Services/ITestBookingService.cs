using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface ITestBookingService
    {
        Task<ResultModel> CreateBookingAsync(int customerId, CreateTestBookingDto createBookingDto);
        Task<ResultModel> GetBookingsByCustomerAsync(int customerId);
        Task<ResultModel> GetBookingByIdAsync(int bookingId);
        Task<ResultModel> GetAllBookingsAsync();
        Task<ResultModel> GetBookingsByStatusAsync(string status);
        Task<ResultModel> UpdateBookingStatusAsync(int bookingId, UpdateBookingStatusDto updateStatusDto);
        Task<ResultModel> GetBookingStatsAsync();
        Task<ResultModel> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ResultModel> GetBookingsReadyForResultsAsync();
        Task<ResultModel> CancelBookingAsync(int bookingId, int customerId);
    }
}