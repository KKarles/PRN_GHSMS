using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Services;
using System.Security.Claims;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TestBookingController : ControllerBase
    {
        private readonly ITestBookingService _testBookingService;

        public TestBookingController(ITestBookingService testBookingService)
        {
            _testBookingService = testBookingService;
        }

        /// <summary>
        /// Create a new test booking (Customer only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateTestBookingDto createBookingDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _testBookingService.CreateBookingAsync(customerId, createBookingDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's own bookings
        /// </summary>
        [HttpGet("my-bookings")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _testBookingService.GetBookingsByCustomerAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get all bookings (Staff, Manager, Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetAllBookings()
        {
            var result = await _testBookingService.GetAllBookingsAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get booking by ID (Staff can see all, Customer can see only their own)
        /// </summary>
        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            var result = await _testBookingService.GetBookingByIdAsync(bookingId);
            
            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new { 
                    success = result.IsSuccess, 
                    message = result.Message 
                });
            }

            // Check if customer is trying to access their own booking
            if (User.IsInRole("Customer"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var booking = result.Data as TestBookingDto;
                if (booking?.CustomerId != customerId)
                {
                    return Forbid("You can only view your own bookings");
                }
            }

            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get bookings by status (Staff, Manager, Admin only)
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetBookingsByStatus(string status)
        {
            var result = await _testBookingService.GetBookingsByStatusAsync(status);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update booking status (Staff only)
        /// Workflow: Booked → SampleCollected → Processing → ResultReady → Completed
        /// </summary>
        [HttpPut("{bookingId}/status")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, [FromBody] UpdateBookingStatusDto updateStatusDto)
        {
            var result = await _testBookingService.UpdateBookingStatusAsync(bookingId, updateStatusDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Cancel booking (Customer can cancel their own bookings if status is 'Booked')
        /// </summary>
        [HttpDelete("{bookingId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _testBookingService.CancelBookingAsync(bookingId, customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message 
            });
        }

        /// <summary>
        /// Get booking statistics (Manager, Admin only)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetBookingStats()
        {
            var result = await _testBookingService.GetBookingStatsAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get bookings by date range (Staff, Manager, Admin only)
        /// </summary>
        [HttpGet("date-range")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetBookingsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _testBookingService.GetBookingsByDateRangeAsync(startDate, endDate);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get bookings ready for results (Staff only)
        /// Shows bookings in 'Processing' status that need results entered
        /// </summary>
        [HttpGet("ready-for-results")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetBookingsReadyForResults()
        {
            var result = await _testBookingService.GetBookingsReadyForResultsAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }
    }
}