using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepo _appointmentRepo;
        private readonly IScheduleRepo _scheduleRepo;
        private readonly IUserRepo _userRepo;
        private readonly IConsultantProfileRepo _consultantProfileRepo;

        public AppointmentService(
            IAppointmentRepo appointmentRepo,
            IScheduleRepo scheduleRepo,
            IUserRepo userRepo,
            IConsultantProfileRepo consultantProfileRepo)
        {
            _appointmentRepo = appointmentRepo;
            _scheduleRepo = scheduleRepo;
            _userRepo = userRepo;
            _consultantProfileRepo = consultantProfileRepo;
        }

        public async Task<ResultModel> GetAvailableSchedulesAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var schedules = await _scheduleRepo.GetConsultantAvailableSchedulesAsync(startDate, endDate);
                var availableSchedules = new List<AvailableScheduleDto>();

                // If no schedules found for the date range, return all users with consultant role
                if (!schedules.Any())
                {
                    var consultantUsers = await _userRepo.GetUsersByRoleAsync("consultant");
                    
                    foreach (var user in consultantUsers)
                    {
                        availableSchedules.Add(new AvailableScheduleDto
                        {
                            ScheduleId = 0, // No specific schedule
                            ConsultantId = user.UserId,
                            ConsultantName = $"{user.FirstName} {user.LastName}",
                            ConsultantSpecialization = user.ConsultantProfile?.Specialization ?? "General",
                            ConsultantExperience = user.ConsultantProfile?.Experience ?? "Not specified",
                            StartTime = startDate,
                            EndTime = endDate,
                            IsAvailable = true
                        });
                    }
                }
                else
                {
                    // Original logic: return actual schedules
                    foreach (var schedule in schedules)
                    {
                        availableSchedules.Add(new AvailableScheduleDto
                        {
                            ScheduleId = schedule.ScheduleId,
                            ConsultantId = schedule.ConsultantId,
                            ConsultantName = $"{schedule.Consultant.FirstName} {schedule.Consultant.LastName}",
                            ConsultantSpecialization = schedule.Consultant.ConsultantProfile?.Specialization ?? "General",
                            ConsultantExperience = schedule.Consultant.ConsultantProfile?.Experience ?? "Not specified",
                            StartTime = schedule.StartTime,
                            EndTime = schedule.EndTime,
                            IsAvailable = schedule.IsAvailable ?? true
                        });
                    }
                }

                return ResultModel.Success(availableSchedules);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get available schedules: {ex.Message}");
            }
        }

        // Other methods would be implemented here...
        public Task<ResultModel> CreateAppointmentAsync(int customerId, CreateAppointmentDto createAppointmentDto)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetAppointmentByIdAsync(int appointmentId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetCustomerAppointmentsAsync(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetConsultantAppointmentsAsync(int consultantId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetAllAppointmentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetAppointmentsByStatusAsync(string status)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto updateStatusDto)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> CancelAppointmentAsync(int appointmentId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetConsultantSchedulesAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetUpcomingAppointmentsAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetAppointmentStatsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<ResultModel> SetMeetingUrlAsync(int appointmentId, string meetingUrl, int consultantId)
        {
            throw new NotImplementedException();
        }
    }
}