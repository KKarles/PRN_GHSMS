namespace Service.DTOs
{
    public class StaffDashboardSummaryDto
    {
        public int TodayAppointmentCount { get; set; }
        public int WaitingForSampleCount { get; set; }
        public int PendingResultEntryCount { get; set; }
        public List<OverdueTaskDto> OverdueTasks { get; set; } = new List<OverdueTaskDto>();
        public List<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
    }

    public class OverdueTaskDto
    {
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime StatusUpdateDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class RecentActivityDto
    {
        public DateTime Timestamp { get; set; }
        public string ActivityText { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public string? UserName { get; set; }
    }
}