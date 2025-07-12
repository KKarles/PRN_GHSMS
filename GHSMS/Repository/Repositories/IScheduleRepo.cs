using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface IScheduleRepo : IGenericRepository<Schedule>
    {
        Task<IEnumerable<Schedule>> GetAvailableSchedulesAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Schedule>> GetSchedulesByConsultantAsync(int consultantId);
        Task<IEnumerable<Schedule>> GetAvailableSchedulesByConsultantAsync(int consultantId, DateTime startDate, DateTime endDate);
        Task<Schedule?> GetScheduleWithAppointmentsAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetSchedulesInTimeRangeAsync(DateTime startTime, DateTime endTime);
        Task<Schedule?> FindAvailableConsultantScheduleAsync(DateTime preferredStartTime, DateTime preferredEndTime);
        Task<IEnumerable<Schedule>> GetConsultantAvailableSchedulesAsync(DateTime startDate, DateTime endDate);
    }
}