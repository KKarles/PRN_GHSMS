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
                            ScheduleId = 0, 
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

        public async Task<ResultModel> CreateAppointmentAsync(int customerId, CreateAppointmentDto createAppointmentDto)
        {
            try
            {
                // Validate customer exists
                var customer = await _userRepo.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                // Validate appointment time is in the future
                if (createAppointmentDto.PreferredStartTime <= DateTime.UtcNow)
                {
                    return ResultModel.BadRequest("Appointment time must be in the future");
                }

                if (createAppointmentDto.PreferredEndTime <= createAppointmentDto.PreferredStartTime)
                {
                    return ResultModel.BadRequest("End time must be after start time");
                }

                Schedule? availableSchedule = null;

                // If preferred consultant is specified, try to find their available schedule
                if (createAppointmentDto.PreferredConsultantId.HasValue)
                {
                    var consultant = await _userRepo.GetByIdAsync(createAppointmentDto.PreferredConsultantId.Value);
                    if (consultant == null)
                    {
                        return ResultModel.BadRequest("Specified consultant not found");
                    }

                    // Check if user has consultant role
                    var consultantWithRoles = await _userRepo.GetUserWithRolesAsync(createAppointmentDto.PreferredConsultantId.Value);
                    if (consultantWithRoles == null || !consultantWithRoles.Roles.Any(r => r.RoleId == 4))
                    {
                        return ResultModel.BadRequest("Specified user is not a consultant");
                    }

                    var consultantSchedules = await _scheduleRepo.GetAvailableSchedulesByConsultantAsync(
                        createAppointmentDto.PreferredConsultantId.Value,
                        createAppointmentDto.PreferredStartTime.Date,
                        createAppointmentDto.PreferredEndTime.Date.AddDays(1));

                    availableSchedule = consultantSchedules.FirstOrDefault(s =>
                        s.StartTime <= createAppointmentDto.PreferredStartTime &&
                        s.EndTime >= createAppointmentDto.PreferredEndTime);

                    // If no specific schedule found, create a flexible appointment
                    if (availableSchedule == null)
                    {
                        // Create a new schedule for this appointment
                        var newSchedule = new Schedule
                        {
                            ConsultantId = createAppointmentDto.PreferredConsultantId.Value,
                            StartTime = createAppointmentDto.PreferredStartTime,
                            EndTime = createAppointmentDto.PreferredEndTime,
                            IsAvailable = true
                        };

                        availableSchedule = await _scheduleRepo.CreateAsync(newSchedule);
                    }
                }
                else
                {
                    // Auto-find available consultant
                    availableSchedule = await _scheduleRepo.FindAvailableConsultantScheduleAsync(
                        createAppointmentDto.PreferredStartTime,
                        createAppointmentDto.PreferredEndTime);

                    // If no schedule found, find any consultant and create schedule
                    if (availableSchedule == null)
                    {
                        var consultants = await _userRepo.GetUsersByRoleAsync("consultant");
                        var firstConsultant = consultants.FirstOrDefault();
                        
                        if (firstConsultant == null)
                        {
                            return ResultModel.BadRequest("No consultants available in the system");
                        }

                        // Create a new schedule with the first available consultant
                        var newSchedule = new Schedule
                        {
                            ConsultantId = firstConsultant.UserId,
                            StartTime = createAppointmentDto.PreferredStartTime,
                            EndTime = createAppointmentDto.PreferredEndTime,
                            IsAvailable = true
                        };

                        availableSchedule = await _scheduleRepo.CreateAsync(newSchedule);
                    }
                }

                if (availableSchedule == null)
                {
                    return ResultModel.BadRequest("Unable to create appointment schedule");
                }

                // Check for conflicting appointments
                var hasConflict = await _appointmentRepo.HasConflictingAppointmentAsync(
                    availableSchedule.ConsultantId, availableSchedule.ScheduleId);

                if (hasConflict)
                {
                    return ResultModel.BadRequest("The selected time slot is no longer available");
                }

                // Generate default meeting URL
                var meetingUrl = GenerateDefaultMeetingUrl(availableSchedule.ConsultantId, customerId);

                var appointment = new Appointment
                {
                    CustomerId = customerId,
                    ConsultantId = availableSchedule.ConsultantId,
                    ScheduleId = availableSchedule.ScheduleId,
                    AppointmentStatus = "Scheduled",
                    MeetingUrl = meetingUrl
                };

                var createdAppointment = await _appointmentRepo.CreateAsync(appointment);
                var appointmentDto = await MapToAppointmentDto(createdAppointment);
                
                return ResultModel.Created(appointmentDto, "Appointment created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to create appointment: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAppointmentByIdAsync(int appointmentId)
        {
            try
            {
                var appointment = await _appointmentRepo.GetAppointmentWithDetailsAsync(appointmentId);
                if (appointment == null)
                {
                    return ResultModel.NotFound("Appointment not found");
                }

                var appointmentDto = await MapToAppointmentDto(appointment);
                return ResultModel.Success(appointmentDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get appointment: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCustomerAppointmentsAsync(int customerId)
        {
            try
            {
                var appointments = await _appointmentRepo.GetAppointmentsByCustomerAsync(customerId);
                var appointmentDtos = new List<AppointmentDto>();

                foreach (var appointment in appointments)
                {
                    appointmentDtos.Add(await MapToAppointmentDto(appointment));
                }

                return ResultModel.Success(appointmentDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get customer appointments: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetConsultantAppointmentsAsync(int consultantId)
        {
            try
            {
                var appointments = await _appointmentRepo.GetAppointmentsByConsultantAsync(consultantId);
                var appointmentDtos = new List<AppointmentDto>();

                foreach (var appointment in appointments)
                {
                    appointmentDtos.Add(await MapToAppointmentDto(appointment));
                }

                return ResultModel.Success(appointmentDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get consultant appointments: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAllAppointmentsAsync()
        {
            try
            {
                var appointments = await _appointmentRepo.GetAppointmentsWithScheduleAndUsersAsync();
                var appointmentDtos = new List<AppointmentDto>();

                foreach (var appointment in appointments)
                {
                    appointmentDtos.Add(await MapToAppointmentDto(appointment));
                }

                return ResultModel.Success(appointmentDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get all appointments: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAppointmentsByStatusAsync(string status)
        {
            try
            {
                // If status is null or empty, get all appointments
                var appointments = await _appointmentRepo.GetAppointmentsByStatusAsync(status);
                var appointmentDtos = new List<AppointmentDto>();

                foreach (var appointment in appointments)
                {
                    appointmentDtos.Add(await MapToAppointmentDto(appointment));
                }

                var message = string.IsNullOrEmpty(status) 
                    ? "All appointments retrieved successfully" 
                    : $"Appointments with status '{status}' retrieved successfully";

                return ResultModel.Success(appointmentDtos, message);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get appointments by status: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto updateStatusDto)
        {
            try
            {
                var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return ResultModel.NotFound("Appointment not found");
                }

                if (!IsValidStatusTransition(appointment.AppointmentStatus, updateStatusDto.AppointmentStatus))
                {
                    return ResultModel.BadRequest($"Invalid status transition from {appointment.AppointmentStatus} to {updateStatusDto.AppointmentStatus}");
                }

                appointment.AppointmentStatus = updateStatusDto.AppointmentStatus;

                if (!string.IsNullOrEmpty(updateStatusDto.MeetingUrl))
                {
                    appointment.MeetingUrl = updateStatusDto.MeetingUrl;
                }

                await _appointmentRepo.UpdateAsync(appointment);
                var appointmentDto = await MapToAppointmentDto(appointment);
                
                return ResultModel.Success(appointmentDto, "Appointment status updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update appointment status: {ex.Message}");
            }
        }

        public async Task<ResultModel> CancelAppointmentAsync(int appointmentId, int userId)
        {
            try
            {
                var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return ResultModel.NotFound("Appointment not found");
                }

                if (appointment.CustomerId != userId && appointment.ConsultantId != userId)
                {
                    return ResultModel.Forbidden("You can only cancel your own appointments");
                }

                if (appointment.AppointmentStatus == "Cancelled" || appointment.AppointmentStatus == "Completed")
                {
                    return ResultModel.BadRequest($"Cannot cancel appointment with status: {appointment.AppointmentStatus}");
                }

                appointment.AppointmentStatus = "Cancelled";
                await _appointmentRepo.UpdateAsync(appointment);

                return ResultModel.Success(null, "Appointment cancelled successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to cancel appointment: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetConsultantSchedulesAsync(int consultantId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var schedules = await _scheduleRepo.GetAvailableSchedulesByConsultantAsync(consultantId, startDate, endDate);
                var consultantSchedules = new List<ScheduleSlotDto>();

                foreach (var schedule in schedules)
                {
                    consultantSchedules.Add(new ScheduleSlotDto
                    {
                        ScheduleId = schedule.ScheduleId,
                        StartTime = schedule.StartTime,
                        EndTime = schedule.EndTime,
                        IsBooked = schedule.Appointments.Any(a => a.AppointmentStatus != "Cancelled")
                    });
                }

                return ResultModel.Success(consultantSchedules);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get consultant schedules: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetUpcomingAppointmentsAsync(int userId)
        {
            try
            {
                var appointments = await _appointmentRepo.GetUpcomingAppointmentsAsync(userId);
                var appointmentDtos = new List<AppointmentDto>();

                foreach (var appointment in appointments)
                {
                    appointmentDtos.Add(await MapToAppointmentDto(appointment));
                }

                return ResultModel.Success(appointmentDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get upcoming appointments: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAppointmentStatsAsync()
        {
            try
            {
                var appointments = await _appointmentRepo.GetAppointmentsWithScheduleAndUsersAsync();
                
                var stats = new AppointmentStatsDto
                {
                    TotalAppointments = appointments.Count(),
                    CompletedAppointments = appointments.Count(a => a.AppointmentStatus == "Completed"),
                    CancelledAppointments = appointments.Count(a => a.AppointmentStatus == "Cancelled"),
                    UpcomingAppointments = appointments.Count(a => a.Schedule.StartTime > DateTime.UtcNow && a.AppointmentStatus != "Cancelled"),
                    AppointmentsByStatus = appointments.GroupBy(a => a.AppointmentStatus)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    AppointmentsByConsultant = appointments.GroupBy(a => $"{a.Consultant.FirstName} {a.Consultant.LastName}")
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return ResultModel.Success(stats);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get appointment stats: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var appointments = await _appointmentRepo.GetAppointmentsByDateRangeAsync(startDate, endDate);
                var appointmentDtos = new List<AppointmentDto>();

                foreach (var appointment in appointments)
                {
                    appointmentDtos.Add(await MapToAppointmentDto(appointment));
                }

                return ResultModel.Success(appointmentDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get appointments by date range: {ex.Message}");
            }
        }

        public async Task<ResultModel> SetMeetingUrlAsync(int appointmentId, string meetingUrl, int consultantId)
        {
            try
            {
                var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return ResultModel.NotFound("Appointment not found");
                }

                if (appointment.ConsultantId != consultantId)
                {
                    return ResultModel.Forbidden("Only the assigned consultant can set the meeting URL");
                }

                appointment.MeetingUrl = meetingUrl;
                await _appointmentRepo.UpdateAsync(appointment);

                return ResultModel.Success(null, "Meeting URL updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to set meeting URL: {ex.Message}");
            }
        }

        // Helper Methods
        private async Task<AppointmentDto> MapToAppointmentDto(Appointment appointment)
        {
            // Ensure we have all the required data
            if (appointment.Customer == null || appointment.Consultant == null || appointment.Schedule == null)
            {
                appointment = await _appointmentRepo.GetAppointmentWithDetailsAsync(appointment.AppointmentId) ?? appointment;
            }

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                CustomerId = appointment.CustomerId,
                CustomerName = appointment.Customer != null ? $"{appointment.Customer.FirstName} {appointment.Customer.LastName}" : "Unknown",
                CustomerEmail = appointment.Customer?.Email ?? "Unknown",
                ConsultantId = appointment.ConsultantId,
                ConsultantName = appointment.Consultant != null ? $"{appointment.Consultant.FirstName} {appointment.Consultant.LastName}" : "Unknown",
                ConsultantSpecialization = appointment.Consultant?.ConsultantProfile?.Specialization ?? "General",
                ScheduleId = appointment.ScheduleId,
                StartTime = appointment.Schedule?.StartTime ?? DateTime.MinValue,
                EndTime = appointment.Schedule?.EndTime ?? DateTime.MinValue,
                AppointmentStatus = appointment.AppointmentStatus,
                MeetingUrl = appointment.MeetingUrl ?? string.Empty,
                CreatedAt = DateTime.UtcNow // Since we don't have CreatedAt in the model
            };
        }

        private string GenerateDefaultMeetingUrl(int consultantId, int customerId)
        {
            // Generate a default meeting URL - this could be integrated with actual video conferencing services
            var meetingId = Guid.NewGuid().ToString("N")[..8];
            return $"https://meet.ghsms.com/room/{consultantId}-{customerId}-{meetingId}";
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, List<string>>
            {
                ["Scheduled"] = new List<string> { "Confirmed", "InProgress", "Cancelled" },
                ["Confirmed"] = new List<string> { "InProgress", "Cancelled" },
                ["InProgress"] = new List<string> { "Completed", "Cancelled" },
                ["Completed"] = new List<string>(), // No transitions from completed
                ["Cancelled"] = new List<string>() // No transitions from cancelled
            };

            return validTransitions.ContainsKey(currentStatus) && 
                   validTransitions[currentStatus].Contains(newStatus);
        }
    }
}