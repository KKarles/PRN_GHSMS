using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface ITestBookingRepo : IGenericRepository<TestBooking>
    {
        Task<IEnumerable<TestBooking>> GetBookingsByCustomerAsync(int customerId);
        Task<IEnumerable<TestBooking>> GetBookingsByStatusAsync(string status);
        Task<IEnumerable<TestBooking>> GetBookingsWithServiceAsync();
        Task<TestBooking?> GetBookingWithDetailsAsync(int bookingId);
        Task<IEnumerable<TestBooking>> GetPaidBookingsAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<IEnumerable<TestBooking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TestBooking>> GetBookingsReadyForResultsAsync();
        Task<int> GetBookingCountByServiceAsync(int serviceId);
    }
}