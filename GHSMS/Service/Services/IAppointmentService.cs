using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IAppointmentService
    {
        Task<ResultModel> CreateAppointmentAsync(int customerId, CreateAppointmentDto createAppointmentDto);
        Task<ResultModel> GetAppointmentByIdAsync(int appointmentId);
        Task<ResultModel> GetCustomerAppointmentsAsync(int customerId);
        Task<ResultModel> GetConsultantAppointmentsAsync(int consultantId);
        Task<ResultModel> GetAllAppointmentsAsync();
        Task<ResultModel> GetAppointmentsByStatusAsync(string status);
        Task<ResultModel> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto updateStatusDto);
        Task<ResultModel> CancelAppointmentAsync(int appointmentId, int userId);
        Task<ResultModel> GetAvailableSchedulesAsync(DateTime startDate, DateTime endDate);
        Task<ResultModel> GetConsultantSchedulesAsync(int consultantId, DateTime startDate, DateTime endDate);
        Task<ResultModel> GetUpcomingAppointmentsAsync(int userId);
        Task<ResultModel> GetAppointmentStatsAsync();
        Task<ResultModel> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ResultModel> SetMeetingUrlAsync(int appointmentId, string meetingUrl, int consultantId);
    }
}