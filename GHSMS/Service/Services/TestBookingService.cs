using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class TestBookingService : ITestBookingService
    {
        private readonly ITestBookingRepo _testBookingRepo;
        private readonly IServiceRepo _serviceRepo;
        private readonly IUserRepo _userRepo;

        public TestBookingService(ITestBookingRepo testBookingRepo, IServiceRepo serviceRepo, IUserRepo userRepo)
        {
            _testBookingRepo = testBookingRepo;
            _serviceRepo = serviceRepo;
            _userRepo = userRepo;
        }

        public async Task<ResultModel> CreateBookingAsync(int customerId, CreateTestBookingDto createBookingDto)
        {
            try
            {
                // Validate customer exists
                var customer = await _userRepo.GetByIdAsync(customerId);
                if (customer == null)
                {
                    return ResultModel.NotFound("Customer not found");
                }

                // Validate service exists
                var service = await _serviceRepo.GetByIdAsync(createBookingDto.ServiceId);
                if (service == null)
                {
                    return ResultModel.NotFound("Service not found");
                }

                // Validate appointment time is in the future
                if (createBookingDto.AppointmentTime <= DateTime.UtcNow)
                {
                    return ResultModel.BadRequest("Appointment time must be in the future");
                }

                var booking = new TestBooking
                {
                    CustomerId = customerId,
                    ServiceId = createBookingDto.ServiceId,
                    AppointmentTime = createBookingDto.AppointmentTime,
                    BookingStatus = "Booked",
                    IsPaid = false,
                    BookedAt = DateTime.UtcNow
                };

                var createdBooking = await _testBookingRepo.CreateAsync(booking);
                var bookingDto = await MapToTestBookingDto(createdBooking);
                return ResultModel.Created(bookingDto, "Booking created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to create booking: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBookingsByCustomerAsync(int customerId)
        {
            try
            {
                var bookings = await _testBookingRepo.GetBookingsByCustomerAsync(customerId);
                var bookingDtos = new List<TestBookingDto>();
                
                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToTestBookingDto(booking));
                }

                return ResultModel.Success(bookingDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get customer bookings: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBookingByIdAsync(int bookingId)
        {
            try
            {
                var booking = await _testBookingRepo.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    return ResultModel.NotFound("Booking not found");
                }

                var bookingDto = await MapToTestBookingDto(booking);
                return ResultModel.Success(bookingDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get booking: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAllBookingsAsync()
        {
            try
            {
                var bookings = await _testBookingRepo.GetBookingsWithServiceAsync();
                var bookingDtos = new List<TestBookingDto>();
                
                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToTestBookingDto(booking));
                }

                return ResultModel.Success(bookingDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get all bookings: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBookingsByStatusAsync(string status)
        {
            try
            {
                var bookings = await _testBookingRepo.GetBookingsByStatusAsync(status);
                var bookingDtos = new List<TestBookingDto>();
                
                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToTestBookingDto(booking));
                }

                return ResultModel.Success(bookingDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get bookings by status: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateBookingStatusAsync(int bookingId, UpdateBookingStatusDto updateStatusDto)
        {
            try
            {
                var booking = await _testBookingRepo.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return ResultModel.NotFound("Booking not found");
                }

                // Validate status transition
                if (!IsValidStatusTransition(booking.BookingStatus, updateStatusDto.BookingStatus))
                {
                    return ResultModel.BadRequest($"Invalid status transition from {booking.BookingStatus} to {updateStatusDto.BookingStatus}");
                }

                booking.BookingStatus = updateStatusDto.BookingStatus;
                
                if (updateStatusDto.IsPaid.HasValue)
                    booking.IsPaid = updateStatusDto.IsPaid.Value;
                
                if (updateStatusDto.ResultDate.HasValue)
                    booking.ResultDate = updateStatusDto.ResultDate.Value;

                await _testBookingRepo.UpdateAsync(booking);
                var bookingDto = await MapToTestBookingDto(booking);
                return ResultModel.Success(bookingDto, "Booking status updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update booking status: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBookingStatsAsync()
        {
            try
            {
                var allBookings = await _testBookingRepo.GetBookingsWithServiceAsync();
                var totalRevenue = await _testBookingRepo.GetTotalRevenueAsync();

                var stats = new BookingStatsDto
                {
                    TotalBookings = allBookings.Count(),
                    PaidBookings = allBookings.Count(b => b.IsPaid),
                    TotalRevenue = totalRevenue,
                    BookingsByStatus = allBookings.GroupBy(b => b.BookingStatus)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    BookingsByService = allBookings.GroupBy(b => b.Service.ServiceName)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return ResultModel.Success(stats);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get booking stats: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var bookings = await _testBookingRepo.GetBookingsByDateRangeAsync(startDate, endDate);
                var bookingDtos = new List<TestBookingDto>();
                
                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToTestBookingDto(booking));
                }

                return ResultModel.Success(bookingDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get bookings by date range: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetBookingsReadyForResultsAsync()
        {
            try
            {
                var bookings = await _testBookingRepo.GetBookingsReadyForResultsAsync();
                var bookingDtos = new List<TestBookingDto>();
                
                foreach (var booking in bookings)
                {
                    bookingDtos.Add(await MapToTestBookingDto(booking));
                }

                return ResultModel.Success(bookingDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get bookings ready for results: {ex.Message}");
            }
        }

        public async Task<ResultModel> CancelBookingAsync(int bookingId, int customerId)
        {
            try
            {
                var booking = await _testBookingRepo.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return ResultModel.NotFound("Booking not found");
                }

                if (booking.CustomerId != customerId)
                {
                    return ResultModel.Forbidden("You can only cancel your own bookings");
                }

                if (booking.BookingStatus != "Booked")
                {
                    return ResultModel.BadRequest("Only bookings with 'Booked' status can be cancelled");
                }

                await _testBookingRepo.RemoveAsync(booking);
                return ResultModel.Success(null, "Booking cancelled successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to cancel booking: {ex.Message}");
            }
        }

        private async Task<TestBookingDto> MapToTestBookingDto(TestBooking booking)
        {
            // Ensure we have the service and customer data
            if (booking.Service == null || booking.Customer == null)
            {
                booking = await _testBookingRepo.GetBookingWithDetailsAsync(booking.BookingId) ?? booking;
            }

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

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, List<string>>
            {
                ["Booked"] = new List<string> { "SampleCollected", "Completed" },
                ["SampleCollected"] = new List<string> { "Processing", "Completed" },
                ["Processing"] = new List<string> { "ResultReady", "Completed" },
                ["ResultReady"] = new List<string> { "Completed" },
                ["Completed"] = new List<string>() // No transitions from completed
            };

            return validTransitions.ContainsKey(currentStatus) && 
                   validTransitions[currentStatus].Contains(newStatus);
        }
    }
}