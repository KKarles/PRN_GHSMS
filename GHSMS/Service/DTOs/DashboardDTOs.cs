namespace Service.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalBookings { get; set; }
        public int BookingsThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int PendingResults { get; set; }
        public Dictionary<string, int> BookingsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BookingsByService { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByService { get; set; } = new Dictionary<string, decimal>();
        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new List<MonthlyRevenueDto>();
    }

    public class MonthlyRevenueDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
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