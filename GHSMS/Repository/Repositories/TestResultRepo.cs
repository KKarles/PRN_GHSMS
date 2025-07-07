using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class TestResultRepo : GenericRepository<TestResult>, ITestResultRepo
    {
        public TestResultRepo(GenderHealthcareDbContext context) : base(context)
        {
        }

        public async Task<TestResult?> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Include(tr => tr.TestResultDetails)
                .Include(tr => tr.Booking)
                    .ThenInclude(b => b.Service)
                .Include(tr => tr.IssuedByNavigation)
                .FirstOrDefaultAsync(tr => tr.BookingId == bookingId);
        }

        public async Task<TestResult?> GetWithDetailsAsync(int resultId)
        {
            return await _dbSet
                .Include(tr => tr.TestResultDetails)
                .Include(tr => tr.Booking)
                    .ThenInclude(b => b.Service)
                .Include(tr => tr.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(tr => tr.IssuedByNavigation)
                .FirstOrDefaultAsync(tr => tr.ResultId == resultId);
        }

        public async Task<IEnumerable<TestResult>> GetResultsByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(tr => tr.TestResultDetails)
                .Include(tr => tr.Booking)
                    .ThenInclude(b => b.Service)
                .Where(tr => tr.Booking.CustomerId == customerId)
                .OrderByDescending(tr => tr.IssuedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestResult>> GetResultsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(tr => tr.TestResultDetails)
                .Include(tr => tr.Booking)
                    .ThenInclude(b => b.Service)
                .Include(tr => tr.Booking)
                    .ThenInclude(b => b.Customer)
                .Where(tr => tr.IssuedAt >= startDate && tr.IssuedAt <= endDate)
                .ToListAsync();
        }
    }
}