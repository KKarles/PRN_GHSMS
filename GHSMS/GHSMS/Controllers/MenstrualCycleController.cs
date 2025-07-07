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
    public class MenstrualCycleController : ControllerBase
    {
        private readonly IMenstrualCycleService _menstrualCycleService;

        public MenstrualCycleController(IMenstrualCycleService menstrualCycleService)
        {
            _menstrualCycleService = menstrualCycleService;
        }

        /// <summary>
        /// Create a new menstrual cycle record (Customer only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateCycle([FromBody] CreateMenstrualCycleDto createCycleDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _menstrualCycleService.CreateCycleAsync(userId, createCycleDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's own menstrual cycles
        /// </summary>
        [HttpGet("my-cycles")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyCycles()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _menstrualCycleService.GetCyclesByUserAsync(userId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's cycle predictions
        /// </summary>
        [HttpGet("predictions")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCyclePredictions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _menstrualCycleService.GetCyclePredictionsAsync(userId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get customer's latest cycle
        /// </summary>
        [HttpGet("latest")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetLatestCycle()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _menstrualCycleService.GetLatestCycleAsync(userId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update a menstrual cycle record
        /// </summary>
        [HttpPut("{cycleId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCycle(int cycleId, [FromBody] UpdateMenstrualCycleDto updateCycleDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _menstrualCycleService.UpdateCycleAsync(cycleId, userId, updateCycleDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Delete a menstrual cycle record
        /// </summary>
        [HttpDelete("{cycleId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteCycle(int cycleId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _menstrualCycleService.DeleteCycleAsync(cycleId, userId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message 
            });
        }

        /// <summary>
        /// Get cycles by date range
        /// </summary>
        [HttpGet("date-range")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCyclesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _menstrualCycleService.GetCyclesByDateRangeAsync(userId, startDate, endDate);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get users needing cycle notifications (Admin/Manager only)
        /// </summary>
        [HttpGet("users-needing-notifications")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetUsersNeedingNotifications()
        {
            var result = await _menstrualCycleService.GetUsersNeedingNotificationsAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get all active cycles (Staff/Manager/Admin only)
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "Staff,Manager,Admin")]
        public async Task<IActionResult> GetActiveCycles()
        {
            var result = await _menstrualCycleService.GetActiveCyclesAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get specific cycle by ID (Customer can only see own, Staff+ can see all)
        /// </summary>
        [HttpGet("{cycleId}")]
        public async Task<IActionResult> GetCycleById(int cycleId)
        {
            var result = await _menstrualCycleService.GetCycleByIdAsync(cycleId);
            
            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new { 
                    success = result.IsSuccess, 
                    message = result.Message 
                });
            }

            // Check if customer is trying to access their own cycle
            if (User.IsInRole("Customer"))
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var cycle = result.Data as MenstrualCycleDto;
                if (cycle?.UserId != customerId)
                {
                    return StatusCode(403, new { 
                        success = false, 
                        message = "You can only view your own cycles" 
                    });
                }
            }

            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }
    }
}