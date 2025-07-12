using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class ScheduleRepo : GenericRepository<Schedule>, IScheduleRepo
    {
        public ScheduleRepo(GenderHealthcareDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Schedule>> GetAvailableSchedulesAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(s => s.Consultant)
                    .ThenInclude(c => c.ConsultantProfile)
                .Include(s => s.Appointments)
                .Where(s => s.IsAvailable == true 
                           && s.StartTime >= startDate 
                           && s.EndTime <= endDate
                           && !s.Appointments.Any(a => a.AppointmentStatus != "Cancelled"))
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByConsultantAsync(int consultantId)
        {
            return await _dbSet
                .Include(s => s.Appointments)
                .Where(s => s.ConsultantId == consultantId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetAvailableSchedulesByConsultantAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(s => s.Appointments)
                .Where(s => s.ConsultantId == consultantId
                           && s.IsAvailable == true
                           && s.StartTime >= startDate
                           && s.EndTime <= endDate
                           && !s.Appointments.Any(a => a.AppointmentStatus != "Cancelled"))
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Schedule?> GetScheduleWithAppointmentsAsync(int scheduleId)
        {
            return await _dbSet
                .Include(s => s.Consultant)
                    .ThenInclude(c => c.ConsultantProfile)
                .Include(s => s.Appointments)
                    .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesInTimeRangeAsync(DateTime startTime, DateTime endTime)
        {
            return await _dbSet
                .Include(s => s.Consultant)
                    .ThenInclude(c => c.ConsultantProfile)
                .Include(s => s.Appointments)
                .Where(s => s.StartTime < endTime && s.EndTime > startTime)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Schedule?> FindAvailableConsultantScheduleAsync(DateTime preferredStartTime, DateTime preferredEndTime)
        {
            // Tìm consultant có lịch trống trong khoảng thời gian yêu cầu
            return await _dbSet
                .Include(s => s.Consultant)
                    .ThenInclude(c => c.ConsultantProfile)
                .Include(s => s.Appointments)
                .Where(s => s.IsAvailable == true
                           && s.StartTime <= preferredStartTime
                           && s.EndTime >= preferredEndTime
                           && !s.Appointments.Any(a => a.AppointmentStatus != "Cancelled"))
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Schedule>> GetConsultantAvailableSchedulesAsync(DateTime startDate, DateTime endDate)
        {
            // Lấy tất cả lịch trống của các consultant có profile
            return await _dbSet
                .Include(s => s.Consultant)
                    .ThenInclude(c => c.ConsultantProfile)
                .Include(s => s.Appointments)
                .Where(s => s.IsAvailable == true
                           && s.StartTime >= startDate
                           && s.EndTime <= endDate
                           && s.Consultant.ConsultantProfile != null
                           && !s.Appointments.Any(a => a.AppointmentStatus != "Cancelled"))
                .OrderBy(s => s.StartTime)
                .ThenBy(s => s.ConsultantId)
                .ToListAsync();
        }
    }
}