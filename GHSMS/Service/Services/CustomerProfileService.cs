using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly IUserRepo _userRepo;
        private readonly ITestBookingRepo _testBookingRepo;
        private readonly ITestResultRepo _testResultRepo;

        public CustomerProfileService(IUserRepo userRepo, ITestBookingRepo testBookingRepo, ITestResultRepo testResultRepo)
        {
            _userRepo = userRepo;
            _testBookingRepo = testBookingRepo;
            _testResultRepo = testResultRepo;
        }

        public async Task<ResultModel> GetCustomerProfileAsync(int customerId)
        {
            try
            {
                var user = await _userRepo.GetUserWithRolesAsync(customerId);
                if (user == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                // Verify user is a customer
                if (!user.Roles.Any(r => r.RoleName == "Customer"))
                {
                    return ResultModel.BadRequest("User is not a customer");
                }

                var customerProfile = MapToCustomerProfileDto(user);
                return ResultModel.Success(customerProfile);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get customer profile: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateCustomerProfileAsync(int customerId, UpdateCustomerProfileDto updateDto)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(customerId);
                if (user == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName.Trim();

                if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                    user.LastName = updateDto.LastName.Trim();

                if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
                    user.PhoneNumber = updateDto.PhoneNumber.Trim();

                if (updateDto.DateOfBirth.HasValue)
                    user.DateOfBirth = DateOnly.FromDateTime(updateDto.DateOfBirth.Value);

                if (!string.IsNullOrWhiteSpace(updateDto.Sex))
                    user.Sex = updateDto.Sex.Trim();

                user.UpdatedAt = DateTime.UtcNow;

                await _userRepo.UpdateAsync(user);

                // Return updated profile
                var updatedUser = await _userRepo.GetUserWithRolesAsync(customerId);
                var customerProfile = MapToCustomerProfileDto(updatedUser!);
                return ResultModel.Success(customerProfile, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update customer profile: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetNotificationSettingsAsync(int customerId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(customerId);
                if (user == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                var settings = new NotificationSettingsDto
                {
                    WantsCycleNotifications = user.WantsCycleNotifications,
                    PillReminderTime = user.PillReminderTime?.ToTimeSpan()
                };

                return ResultModel.Success(settings);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get notification settings: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateNotificationSettingsAsync(int customerId, NotificationSettingsDto settingsDto)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(customerId);
                if (user == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                user.WantsCycleNotifications = settingsDto.WantsCycleNotifications;
                user.PillReminderTime = settingsDto.PillReminderTime.HasValue 
                    ? TimeOnly.FromTimeSpan(settingsDto.PillReminderTime.Value) 
                    : null;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepo.UpdateAsync(user);

                var updatedSettings = new NotificationSettingsDto
                {
                    WantsCycleNotifications = user.WantsCycleNotifications,
                    PillReminderTime = user.PillReminderTime?.ToTimeSpan()
                };

                return ResultModel.Success(updatedSettings, "Notification settings updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update notification settings: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCustomerDashboardAsync(int customerId)
        {
            try
            {
                var user = await _userRepo.GetUserWithRolesAsync(customerId);
                if (user == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                // Get customer bookings
                var bookings = await _testBookingRepo.GetBookingsByCustomerAsync(customerId);
                var bookingsList = bookings.ToList();

                // Get customer test results
                var results = await _testResultRepo.GetResultsByCustomerAsync(customerId);

                // Calculate statistics
                var totalBookings = bookingsList.Count;
                var completedTests = bookingsList.Count(b => b.BookingStatus == "Completed");
                var pendingResults = bookingsList.Count(b => b.BookingStatus == "Processing" || b.BookingStatus == "SampleCollected");
                var totalSpent = bookingsList.Where(b => b.IsPaid).Sum(b => b.Service?.Price ?? 0);
                var lastVisit = bookingsList.Where(b => b.BookingStatus == "Completed")
                    .OrderByDescending(b => b.BookedAt)
                    .FirstOrDefault()?.BookedAt;
                var nextAppointment = bookingsList.Where(b => b.AppointmentTime > DateTime.UtcNow)
                    .OrderBy(b => b.AppointmentTime)
                    .FirstOrDefault()?.AppointmentTime;

                // Get recent bookings (last 5)
                var recentBookings = bookingsList
                    .OrderByDescending(b => b.BookedAt)
                    .Take(5)
                    .Select(MapToTestBookingDto)
                    .ToList();

                var dashboard = new CustomerDashboardSummaryDto
                {
                    Profile = MapToCustomerProfileDto(user),
                    TotalBookings = totalBookings,
                    CompletedTests = completedTests,
                    PendingResults = pendingResults,
                    TotalSpent = totalSpent,
                    LastVisit = lastVisit,
                    NextAppointment = nextAppointment,
                    RecentBookings = recentBookings,
                    NotificationSettings = new NotificationSettingsDto
                    {
                        WantsCycleNotifications = user.WantsCycleNotifications,
                        PillReminderTime = user.PillReminderTime?.ToTimeSpan()
                    }
                };

                return ResultModel.Success(dashboard);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get customer dashboard: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteCustomerAccountAsync(int customerId)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(customerId);
                if (user == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                // Check if customer has any active bookings
                var activeBookings = await _testBookingRepo.GetBookingsByCustomerAsync(customerId);
                var hasActiveBookings = activeBookings.Any(b => 
                    b.BookingStatus == "Booked" || 
                    b.BookingStatus == "SampleCollected" || 
                    b.BookingStatus == "Processing");

                if (hasActiveBookings)
                {
                    return ResultModel.BadRequest("Cannot delete account with active bookings. Please complete or cancel all active bookings first.");
                }

                await _userRepo.RemoveAsync(user);
                return ResultModel.Success(null, "Customer account deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to delete customer account: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCustomerBookingHistoryAsync(int customerId)
        {
            try
            {
                var bookings = await _testBookingRepo.GetBookingsByCustomerAsync(customerId);
                var bookingDtos = bookings.Select(MapToTestBookingDto).ToList();
                return ResultModel.Success(bookingDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get customer booking history: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCustomerTestResultsAsync(int customerId)
        {
            try
            {
                var results = await _testResultRepo.GetResultsByCustomerAsync(customerId);
                var resultDtos = results.Select(MapToTestResultDto).ToList();
                return ResultModel.Success(resultDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get customer test results: {ex.Message}");
            }
        }

        private CustomerProfileDto MapToCustomerProfileDto(Repository.Models.User user)
        {
            return new CustomerProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                Sex = user.Sex,
                WantsCycleNotifications = user.WantsCycleNotifications,
                PillReminderTime = user.PillReminderTime?.ToTimeSpan(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>()
            };
        }

        private TestBookingDto MapToTestBookingDto(Repository.Models.TestBooking booking)
        {
            var book = booking;
            var name = book.Customer.FirstName;
            return new TestBookingDto
            {
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId,
                CustomerName = booking.Customer != null ? $"{booking.Customer.FirstName} {booking.Customer.LastName}" : "Unknown",
                ServiceId = booking.ServiceId,
                ServiceName = booking.Service?.ServiceName ?? "Unknown Service",
                ServicePrice = booking.Service?.Price ?? 0,
                AppointmentTime = booking.AppointmentTime,
                BookingStatus = booking.BookingStatus,
                IsPaid = booking.IsPaid,
                BookedAt = booking.BookedAt ?? DateTime.UtcNow,
                ResultDate = booking.ResultDate
            };
        }

        private TestResultDto MapToTestResultDto(Repository.Models.TestResult result)
        {
            return new TestResultDto
            {
                ResultId = result.ResultId,
                BookingId = result.BookingId,
                CustomerName = result.Booking?.Customer != null ? 
                    $"{result.Booking.Customer.FirstName} {result.Booking.Customer.LastName}" : "Unknown",
                ServiceName = result.Booking?.Service?.ServiceName ?? "Unknown Service",
                Notes = result.Notes,
                IssuedByName = result.IssuedByNavigation != null ? 
                    $"{result.IssuedByNavigation.FirstName} {result.IssuedByNavigation.LastName}" : "Unknown",
                IssuedAt = result.IssuedAt ?? DateTime.UtcNow,
                ResultDetails = result.TestResultDetails?.Select(rd => new TestResultDetailDto
                {
                    AnalyteName = rd.AnalyteName,
                    Value = rd.Value,
                    Unit = rd.Unit,
                    ReferenceRange = rd.ReferenceRange,
                    Flag = rd.Flag
                }).ToList() ?? new List<TestResultDetailDto>()
            };
        }
    }
}