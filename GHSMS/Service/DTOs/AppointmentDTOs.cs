using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class CreateAppointmentDto
    {
        [Required]
        public DateTime PreferredStartTime { get; set; }

        [Required]
        public DateTime PreferredEndTime { get; set; }

        public int? PreferredConsultantId { get; set; }

        public string? Notes { get; set; }
    }

    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int ConsultantId { get; set; }
        public string ConsultantName { get; set; } = string.Empty;
        public string ConsultantSpecialization { get; set; } = string.Empty;
        public int ScheduleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string AppointmentStatus { get; set; } = string.Empty;
        public string MeetingUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        public string AppointmentStatus { get; set; } = string.Empty;

        public string? MeetingUrl { get; set; }
    }

    public class AvailableScheduleDto
    {
        public int ScheduleId { get; set; }
        public int ConsultantId { get; set; }
        public string ConsultantName { get; set; } = string.Empty;
        public string ConsultantSpecialization { get; set; } = string.Empty;
        public string ConsultantExperience { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AppointmentStatsDto
    {
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public Dictionary<string, int> AppointmentsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AppointmentsByConsultant { get; set; } = new Dictionary<string, int>();
    }

    public class ConsultantScheduleDto
    {
        public int ConsultantId { get; set; }
        public string ConsultantName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public List<ScheduleSlotDto> AvailableSlots { get; set; } = new List<ScheduleSlotDto>();
    }

    public class ScheduleSlotDto
    {
        public int ScheduleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; }
    }
}