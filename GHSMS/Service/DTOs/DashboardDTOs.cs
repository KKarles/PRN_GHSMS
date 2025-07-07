namespace Service.DTOs
{
    public class DashboardStatsDto
    {
        public RevenueStatsDto Revenue { get; set; } = new RevenueStatsDto();
        public BookingStatsDto Bookings { get; set; } = new BookingStatsDto();
        public UserStatsDto Users { get; set; } = new UserStatsDto();
        public ServiceStatsDto Services { get; set; } = new ServiceStatsDto();
    }

    public class RevenueStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal DailyRevenue { get; set; }
        public List<MonthlyRevenueDto> MonthlyTrends { get; set; } = new List<MonthlyRevenueDto>();
        public List<ServiceRevenueDto> RevenueByService { get; set; } = new List<ServiceRevenueDto>();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class ServiceRevenueDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int ActiveUsers { get; set; }
        public Dictionary<string, int> UsersByRole { get; set; } = new Dictionary<string, int>();
        public List<DailyRegistrationDto> RegistrationTrends { get; set; } = new List<DailyRegistrationDto>();
    }

    public class DailyRegistrationDto
    {
        public DateTime Date { get; set; }
        public int RegistrationCount { get; set; }
    }

    public class ServiceStatsDto
    {
        public int TotalServices { get; set; }
        public List<PopularServiceDto> PopularServices { get; set; } = new List<PopularServiceDto>();
        public Dictionary<string, int> ServicesByType { get; set; } = new Dictionary<string, int>();
    }

    public class PopularServiceDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerDashboardDto
    {
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int PendingResults { get; set; }
        public decimal TotalSpent { get; set; }
        public List<TestBookingDto> RecentBookings { get; set; } = new List<TestBookingDto>();
        public List<TestResultDto> RecentResults { get; set; } = new List<TestResultDto>();
        public MenstrualCycleDto? CurrentCycle { get; set; }
        public CyclePredictionDto? CyclePredictions { get; set; }
    }
}