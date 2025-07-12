using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class AppointmentRepo : GenericRepository<Appointment>, IAppointmentRepo
    {
        public AppointmentRepo(GenderHealthcareDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(a => a.Consultant)
                .Include(a => a.Schedule)
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.Schedule.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByConsultantAsync(int consultantId)
        {
            return await _dbSet
                .Include(a => a.Customer)
                .Include(a => a.Schedule)
                .Where(a => a.ConsultantId == consultantId)
                .OrderByDescending(a => a.Schedule.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(string status)
        {
            var query = _dbSet
                .Include(a => a.Customer)
                .Include(a => a.Consultant)
                .Include(a => a.Schedule);

            // If status is null or empty, return all appointments
            if (status == "all")
            {
                return await query.ToListAsync();
            }

            // Otherwise filter by the specific status
            return await query
                .Where(a => a.AppointmentStatus == status)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId)
        {
            return await _dbSet
                .Include(a => a.Customer)
                .Include(a => a.Consultant)
                    .ThenInclude(c => c.ConsultantProfile)
                .Include(a => a.Schedule)
                .Include(a => a.Feedbacks)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(a => a.Customer)
                .Include(a => a.Consultant)
                .Include(a => a.Schedule)
                .Where(a => a.Schedule.StartTime >= startDate && a.Schedule.StartTime <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(a => a.Customer)
                .Include(a => a.Consultant)
                .Include(a => a.Schedule)
                .Where(a => (a.CustomerId == userId || a.ConsultantId == userId) 
                           && a.Schedule.StartTime > now
                           && a.AppointmentStatus != "Cancelled")
                .OrderBy(a => a.Schedule.StartTime)
                .ToListAsync();
        }

        public async Task<bool> HasConflictingAppointmentAsync(int consultantId, int scheduleId)
        {
            return await _dbSet
                .AnyAsync(a => a.ConsultantId == consultantId 
                              && a.ScheduleId == scheduleId 
                              && a.AppointmentStatus != "Cancelled");
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsWithScheduleAndUsersAsync()
        {
            return await _dbSet
                .Include(a => a.Customer)
                .Include(a => a.Consultant)
                    .ThenInclude(c => c.ConsultantProfile)
                .Include(a => a.Schedule)
                .ToListAsync();
        }
    }
}