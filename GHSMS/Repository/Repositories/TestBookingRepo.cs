using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class TestBookingRepo : GenericRepository<TestBooking>, ITestBookingRepo
    {
        public TestBookingRepo(GenderHealthcareDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TestBooking>> GetBookingsByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(tb => tb.Service)
                .Where(tb => tb.CustomerId == customerId)
                .OrderByDescending(tb => tb.BookedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestBooking>> GetBookingsByStatusAsync(string status)
        {
            return await _dbSet
                .Include(tb => tb.Service)
                .Include(tb => tb.Customer)
                .Where(tb => tb.BookingStatus == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestBooking>> GetBookingsWithServiceAsync()
        {
            return await _dbSet
                .Include(tb => tb.Service)
                .Include(tb => tb.Customer)
                .ToListAsync();
        }

        public async Task<TestBooking?> GetBookingWithDetailsAsync(int bookingId)
        {
            return await _dbSet
                .Include(tb => tb.Service)
                    .ThenInclude(s => s.Analytes)
                .Include(tb => tb.Customer)
                .Include(tb => tb.TestResult)
                .FirstOrDefaultAsync(tb => tb.BookingId == bookingId);
        }

        public async Task<IEnumerable<TestBooking>> GetPaidBookingsAsync()
        {
            return await _dbSet
                .Include(tb => tb.Service)
                .Where(tb => tb.IsPaid)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _dbSet
                .Include(tb => tb.Service)
                .Where(tb => tb.IsPaid)
                .SumAsync(tb => tb.Service.Price);
        }

        public async Task<IEnumerable<TestBooking>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(tb => tb.Service)
                .Include(tb => tb.Customer)
                .Where(tb => tb.BookedAt >= startDate && tb.BookedAt <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestBooking>> GetBookingsReadyForResultsAsync()
        {
            return await _dbSet
                .Include(tb => tb.Service)
                    .ThenInclude(s => s.Analytes)
                .Include(tb => tb.Customer)
                .Where(tb => tb.BookingStatus == "Processing")
                .ToListAsync();
        }

        public async Task<int> GetBookingCountByServiceAsync(int serviceId)
        {
            return await _dbSet
                .CountAsync(tb => tb.ServiceId == serviceId);
        }
    }
}