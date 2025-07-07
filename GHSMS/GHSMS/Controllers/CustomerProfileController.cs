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
    public class CustomerProfileController : ControllerBase
    {
        private readonly ICustomerProfileService _customerProfileService;

        public CustomerProfileController(ICustomerProfileService customerProfileService)
        {
            _customerProfileService = customerProfileService;
        }

        /// <summary>
        /// Get customer's own profile information
        /// </summary>
        [HttpGet("profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.GetCustomerProfileAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update customer's own profile information
        /// </summary>
        [HttpPut("profile")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateCustomerProfileDto updateDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.UpdateCustomerProfileAsync(customerId, updateDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's notification settings
        /// </summary>
        [HttpGet("notifications")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.GetNotificationSettingsAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update customer's notification settings (cycle notifications and pill reminders)
        /// </summary>
        [HttpPut("notifications")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsDto settingsDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.UpdateNotificationSettingsAsync(customerId, settingsDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's dashboard summary (profile + booking stats)
        /// </summary>
        [HttpGet("dashboard")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCustomerDashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.GetCustomerDashboardAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's booking history
        /// </summary>
        [HttpGet("bookings")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyBookingHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.GetCustomerBookingHistoryAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's test results
        /// </summary>
        [HttpGet("results")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyTestResults()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.GetCustomerTestResultsAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Delete customer's own account (with validation for active bookings)
        /// </summary>
        [HttpDelete("account")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _customerProfileService.DeleteCustomerAccountAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message 
            });
        }

        /// <summary>
        /// Get specific customer profile (Staff, Manager, Admin only)
        /// </summary>
        [HttpGet("{customerId}/profile")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetCustomerProfile(int customerId)
        {
            var result = await _customerProfileService.GetCustomerProfileAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get specific customer's dashboard (Staff, Manager, Admin only)
        /// </summary>
        [HttpGet("{customerId}/dashboard")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetCustomerDashboard(int customerId)
        {
            var result = await _customerProfileService.GetCustomerDashboardAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }
    }
}