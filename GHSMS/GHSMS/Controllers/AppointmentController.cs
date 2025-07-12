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
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        /// <summary>
        /// Create a new appointment (Customer only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var result = await _appointmentService.CreateAppointmentAsync(userId.Value, createAppointmentDto);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get appointment by ID
        /// </summary>
        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetAppointment(int appointmentId)
        {
            var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get current user's appointments
        /// </summary>
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var userRole = GetCurrentUserRole();
            
            if (userRole == "Consultant")
            {
                var result = await _appointmentService.GetConsultantAppointmentsAsync(userId.Value);
                return StatusCode(result.Code, result);
            }
            else
            {
                var result = await _appointmentService.GetCustomerAppointmentsAsync(userId.Value);
                return StatusCode(result.Code, result);
            }
        }

        /// <summary>
        /// Get upcoming appointments for current user
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var result = await _appointmentService.GetUpcomingAppointmentsAsync(userId.Value);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get all appointments (Admin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var result = await _appointmentService.GetAllAppointmentsAsync();
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get appointments by status (Admin/Consultant only)
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Consultant")]
        public async Task<IActionResult> GetAppointmentsByStatus(string status)
        {
            var result = await _appointmentService.GetAppointmentsByStatusAsync(status);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get appointments by date range (Admin only)
        /// </summary>
        [HttpGet("date-range")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointmentsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _appointmentService.GetAppointmentsByDateRangeAsync(startDate, endDate);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get available schedules for booking
        /// </summary>
        [HttpGet("available-schedules")]
        public async Task<IActionResult> GetAvailableSchedules([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _appointmentService.GetAvailableSchedulesAsync(startDate, endDate);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get consultant schedules
        /// </summary>
        [HttpGet("consultant/{consultantId}/schedules")]
        public async Task<IActionResult> GetConsultantSchedules(int consultantId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _appointmentService.GetConsultantSchedulesAsync(consultantId, startDate, endDate);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Update appointment status (Consultant/Admin only)
        /// </summary>
        [HttpPut("{appointmentId}/status")]
        [Authorize(Roles = "Consultant,Admin")]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, [FromBody] UpdateAppointmentStatusDto updateStatusDto)
        {
            var result = await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, updateStatusDto);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Set meeting URL for appointment (Consultant only)
        /// </summary>
        [HttpPut("{appointmentId}/meeting-url")]
        [Authorize(Roles = "Consultant")]
        public async Task<IActionResult> SetMeetingUrl(int appointmentId, [FromBody] string meetingUrl)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var result = await _appointmentService.SetMeetingUrlAsync(appointmentId, meetingUrl, userId.Value);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Cancel appointment
        /// </summary>
        [HttpDelete("{appointmentId}")]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var result = await _appointmentService.CancelAppointmentAsync(appointmentId, userId.Value);
            return StatusCode(result.Code, result);
        }

        /// <summary>
        /// Get appointment statistics (Admin only)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointmentStats()
        {
            var result = await _appointmentService.GetAppointmentStatsAsync();
            return StatusCode(result.Code, result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        private string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}