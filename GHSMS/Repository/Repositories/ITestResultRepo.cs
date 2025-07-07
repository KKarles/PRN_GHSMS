using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface ITestResultRepo : IGenericRepository<TestResult>
    {
        Task<TestResult?> GetByBookingIdAsync(int bookingId);
        Task<TestResult?> GetWithDetailsAsync(int resultId);
        Task<IEnumerable<TestResult>> GetResultsByCustomerAsync(int customerId);
        Task<IEnumerable<TestResult>> GetResultsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}