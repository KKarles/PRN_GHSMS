using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class StaffDashboardService : IStaffDashboardService
    {
        private readonly ITestBookingRepo _testBookingRepo;
        private readonly IUserRepo _userRepo;

        public StaffDashboardService(ITestBookingRepo testBookingRepo, IUserRepo userRepo)
        {
            _testBookingRepo = testBookingRepo;
            _userRepo = userRepo;
        }

        public async Task<ResultModel> GetDashboardSummaryAsync()
        {
            try
            {
                var today = DateTime.Today;

                // Get all bookings for calculations
                var allBookings = await _testBookingRepo.GetAllAsync(
                    b => b.Service,
                    b => b.Customer
                );

                // Today's appointment count
                var todayAppointmentCount = allBookings.Count(b => 
                    b.AppointmentTime.HasValue && b.AppointmentTime.Value.Date == today);

                // Waiting for sample count (Booked status + today's appointments)
                var waitingForSampleCount = allBookings.Count(b => 
                    b.BookingStatus == "Booked" && b.AppointmentTime.HasValue && b.AppointmentTime.Value.Date == today);

                // Pending result entry count (SampleCollected status)
                var pendingResultEntryCount = allBookings.Count(b => 
                    b.BookingStatus == "SampleCollected");

                // Overdue tasks (Processing status for more than 3 days)
                var threeDaysAgo = DateTime.Now.AddDays(-3);
                var overdueTasks = allBookings
                    .Where(b => b.BookingStatus == "Processing" && 
                               b.BookedAt.HasValue && b.BookedAt.Value < threeDaysAgo)
                    .Select(b => new OverdueTaskDto
                    {
                        BookingId = b.BookingId,
                        CustomerName = $"{b.Customer.FirstName} {b.Customer.LastName}",
                        ServiceName = b.Service.ServiceName,
                        StatusUpdateDate = b.BookedAt ?? DateTime.Now,
                        DaysOverdue = b.BookedAt.HasValue ? (int)(DateTime.Now - b.BookedAt.Value).TotalDays : 0
                    })
                    .OrderByDescending(t => t.DaysOverdue)
                    .ToList();

                // Recent activities (last 10 booking updates)
                var recentActivities = allBookings
                    .Where(b => b.BookedAt.HasValue)
                    .OrderByDescending(b => b.BookedAt)
                    .Take(10)
                    .Select(b => new RecentActivityDto
                    {
                        Timestamp = b.BookedAt ?? DateTime.Now,
                        ActivityText = GenerateActivityText(b),
                        ActivityType = "BookingUpdate",
                        BookingId = b.BookingId,
                        UserName = "System" // In a real system, you'd track who made the update
                    })
                    .ToList();

                var summary = new StaffDashboardSummaryDto
                {
                    TodayAppointmentCount = todayAppointmentCount,
                    WaitingForSampleCount = waitingForSampleCount,
                    PendingResultEntryCount = pendingResultEntryCount,
                    OverdueTasks = overdueTasks,
                    RecentActivities = recentActivities
                };

                return ResultModel.Success(summary);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get staff dashboard summary: {ex.Message}");
            }
        }

        private string GenerateActivityText(Repository.Models.TestBooking booking)
        {
            var customerName = $"{booking.Customer.FirstName} {booking.Customer.LastName}";
            
            return booking.BookingStatus switch
            {
                "Booked" => $"New booking created for {customerName} - {booking.Service.ServiceName}",
                "SampleCollected" => $"Sample collected for {customerName} - Booking #{booking.BookingId}",
                "Processing" => $"Results processing started for {customerName} - Booking #{booking.BookingId}",
                "ResultReady" => $"Results ready for {customerName} - Booking #{booking.BookingId}",
                _ => $"Booking #{booking.BookingId} status updated to {booking.BookingStatus}"
            };
        }
    }
}