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
    public class StaffQualificationController : ControllerBase
    {
        private readonly IStaffQualificationService _staffQualificationService;

        public StaffQualificationController(IStaffQualificationService staffQualificationService)
        {
            _staffQualificationService = staffQualificationService;
        }

        /// <summary>
        /// Get own staff qualification profile (Staff, Consultant only)
        /// </summary>
        [HttpGet("my-qualifications")]
        [Authorize(Roles = "Staff,Consultant")]
        public async Task<IActionResult> GetMyQualifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int staffId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _staffQualificationService.GetStaffQualificationAsync(staffId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Create own staff qualification profile (Staff, Consultant only)
        /// </summary>
        [HttpPost("my-qualifications")]
        [Authorize(Roles = "Staff,Consultant")]
        public async Task<IActionResult> CreateMyQualifications([FromBody] CreateStaffQualificationDto createDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int staffId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _staffQualificationService.CreateStaffQualificationAsync(staffId, createDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update own staff qualification profile (Staff, Consultant only)
        /// </summary>
        [HttpPut("my-qualifications")]
        [Authorize(Roles = "Staff,Consultant")]
        public async Task<IActionResult> UpdateMyQualifications([FromBody] UpdateStaffQualificationDto updateDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int staffId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _staffQualificationService.UpdateStaffQualificationAsync(staffId, updateDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Delete own staff qualification profile (Staff, Consultant only)
        /// </summary>
        [HttpDelete("my-qualifications")]
        [Authorize(Roles = "Staff,Consultant")]
        public async Task<IActionResult> DeleteMyQualifications()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int staffId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _staffQualificationService.DeleteStaffQualificationAsync(staffId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message 
            });
        }

        /// <summary>
        /// Get all staff qualifications (Manager, Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetAllStaffQualifications()
        {
            var result = await _staffQualificationService.GetAllStaffQualificationsAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get specific staff qualification by ID (Manager, Admin only)
        /// </summary>
        [HttpGet("{staffId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetStaffQualification(int staffId)
        {
            var result = await _staffQualificationService.GetStaffQualificationAsync(staffId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Create staff qualification for specific staff member (Manager, Admin only)
        /// </summary>
        [HttpPost("{staffId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> CreateStaffQualification(int staffId, [FromBody] CreateStaffQualificationDto createDto)
        {
            var result = await _staffQualificationService.CreateStaffQualificationAsync(staffId, createDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update staff qualification for specific staff member (Manager, Admin only)
        /// </summary>
        [HttpPut("{staffId}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateStaffQualification(int staffId, [FromBody] UpdateStaffQualificationDto updateDto)
        {
            var result = await _staffQualificationService.UpdateStaffQualificationAsync(staffId, updateDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Delete staff qualification for specific staff member (Admin only)
        /// </summary>
        [HttpDelete("{staffId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStaffQualification(int staffId)
        {
            var result = await _staffQualificationService.DeleteStaffQualificationAsync(staffId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message 
            });
        }

        /// <summary>
        /// Get staff by specialization (Public access for customer information)
        /// </summary>
        [HttpGet("specialization/{specialization}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStaffBySpecialization(string specialization)
        {
            var result = await _staffQualificationService.GetStaffBySpecializationAsync(specialization);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get staff without qualification profiles (Manager, Admin only)
        /// </summary>
        [HttpGet("without-qualifications")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetStaffWithoutQualifications()
        {
            var result = await _staffQualificationService.GetStaffWithoutQualificationsAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }
    }
}