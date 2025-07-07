using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get comprehensive dashboard statistics (Manager, Admin only)
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _dashboardService.GetDashboardStatsAsync();
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get revenue statistics (Manager, Admin only)
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStats()
        {
            var result = await _dashboardService.GetRevenueStatsAsync();
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get user statistics (Manager, Admin only)
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetUserStats()
        {
            var result = await _dashboardService.GetUserStatsAsync();
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get booking statistics (Manager, Admin only)
        /// </summary>
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookingStats()
        {
            var result = await _dashboardService.GetBookingStatsAsync();
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get service statistics (Manager, Admin only)
        /// </summary>
        [HttpGet("services")]
        public async Task<IActionResult> GetServiceStats()
        {
            var result = await _dashboardService.GetServiceStatsAsync();
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get monthly revenue for a specific year (Manager, Admin only)
        /// </summary>
        [HttpGet("revenue/monthly/{year}")]
        public async Task<IActionResult> GetMonthlyRevenue(int year)
        {
            var result = await _dashboardService.GetMonthlyRevenueAsync(year);
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get revenue breakdown by service (Manager, Admin only)
        /// </summary>
        [HttpGet("revenue/by-service")]
        public async Task<IActionResult> GetRevenueByService()
        {
            var result = await _dashboardService.GetRevenueByServiceAsync();
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get user registration trends (Manager, Admin only)
        /// </summary>
        [HttpGet("users/registration-trends")]
        public async Task<IActionResult> GetRegistrationTrends([FromQuery] int days = 30)
        {
            var result = await _dashboardService.GetRegistrationTrendsAsync(days);
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        /// <summary>
        /// Get popular services (Manager, Admin only)
        /// </summary>
        [HttpGet("services/popular")]
        public async Task<IActionResult> GetPopularServices([FromQuery] int limit = 10)
        {
            var result = await _dashboardService.GetPopularServicesAsync(limit);
            return StatusCode(result.Code, new
            {
                success = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }
    }
}