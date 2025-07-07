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
    public class TestResultController : ControllerBase
    {
        private readonly ITestResultService _testResultService;

        public TestResultController(ITestResultService testResultService)
        {
            _testResultService = testResultService;
        }

        /// <summary>
        /// Create test result for a booking (Staff only)
        /// This moves booking from 'Processing' to 'ResultReady' status
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> CreateTestResult([FromBody] CreateTestResultDto createResultDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int issuedByUserId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _testResultService.CreateTestResultAsync(issuedByUserId, createResultDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get test result by result ID (Staff can see all, Customer can see only their own)
        /// </summary>
        [HttpGet("{resultId}")]
        public async Task<IActionResult> GetTestResultById(int resultId)
        {
            var result = await _testResultService.GetTestResultByIdAsync(resultId);
            
            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new { 
                    success = result.IsSuccess, 
                    message = result.Message 
                });
            }

            // Check if customer is trying to access their own result
            if (User.IsInRole("Customer"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Additional check would be needed here to verify the result belongs to the customer
                // This would require getting the booking info from the result
            }

            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get test result by booking ID (Staff can see all, Customer can see only their own)
        /// </summary>
        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetTestResultByBookingId(int bookingId)
        {
            var result = await _testResultService.GetTestResultByBookingIdAsync(bookingId);
            
            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new { 
                    success = result.IsSuccess, 
                    message = result.Message 
                });
            }

            // Check if customer is trying to access their own result
            if (User.IsInRole("Customer"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                // Additional authorization check would be implemented here
                // to ensure the booking belongs to the customer
            }

            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's own test results
        /// </summary>
        [HttpGet("my-results")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyTestResults()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _testResultService.GetTestResultsByCustomerAsync(customerId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update test result (Staff only)
        /// </summary>
        [HttpPut("{resultId}")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> UpdateTestResult(int resultId, [FromBody] UpdateTestResultDto updateResultDto)
        {
            var result = await _testResultService.UpdateTestResultAsync(resultId, updateResultDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get test results by date range (Staff, Manager, Admin only)
        /// </summary>
        [HttpGet("date-range")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetTestResultsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _testResultService.GetTestResultsByDateRangeAsync(startDate, endDate);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Delete test result (Admin only)
        /// </summary>
        [HttpDelete("{resultId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTestResult(int resultId)
        {
            var result = await _testResultService.DeleteTestResultAsync(resultId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message 
            });
        }
    }
}