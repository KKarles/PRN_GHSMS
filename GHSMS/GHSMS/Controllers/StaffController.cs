using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Staff,Manager,Admin")]
    public class StaffController : ControllerBase
    {
        private readonly IStaffDashboardService _staffDashboardService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffDashboardService staffDashboardService, ILogger<StaffController> logger)
        {
            _staffDashboardService = staffDashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive dashboard summary for staff
        /// </summary>
        [HttpGet("dashboard-summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var result = await _staffDashboardService.GetDashboardSummaryAsync();
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff dashboard summary");
                return StatusCode(500, new { message = "Failed to get dashboard summary" });
            }
        }
    }
}