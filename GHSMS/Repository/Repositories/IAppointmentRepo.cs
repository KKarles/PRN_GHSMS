using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface IAppointmentRepo : IGenericRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(int customerId);
        Task<IEnumerable<Appointment>> GetAppointmentsByConsultantAsync(int consultantId);
        Task<IEnumerable<Appointment>> GetAppointmentsByStatusAsync(string status);
        Task<Appointment?> GetAppointmentWithDetailsAsync(int appointmentId);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int userId);
        Task<bool> HasConflictingAppointmentAsync(int consultantId, int scheduleId);
        Task<IEnumerable<Appointment>> GetAppointmentsWithScheduleAndUsersAsync();
    }
}