using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Staff,Manager,Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserLookupService _userLookupService;
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserLookupService userLookupService, IUserService userService, ILogger<UsersController> logger)
        {
            _userLookupService = userLookupService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated and searchable list of all users
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? role = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                var result = await _userLookupService.GetAllUsersAsync(role, search, page, limit);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(500, new { message = "Failed to get users list" });
            }
        }

        /// <summary>
        /// Get all employees (users with Staff or Consultant roles)
        /// </summary>
        [HttpGet("employees")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var result = await _userService.GetEmployeesAsync();
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employees list");
                return StatusCode(500, new { message = "Failed to get employees list" });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var result = await _userService.GetUserProfileAsync(id);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by ID {id}");
                return StatusCode(500, new { message = "Failed to get user info" });
            }
        }

        /// <summary>
        /// Get all staff users
        /// </summary>
        [HttpGet("staff")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetStaff()
        {
            try
            {
                var result = await _userService.GetStaffAsync();
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting staff list");
                return StatusCode(500, new { message = "Failed to get staff list" });
            }
        }

        /// <summary>
        /// Get all consultant users
        /// </summary>
        [HttpGet("consultants")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetConsultants()
        {
            try
            {
                var result = await _userService.GetConsultantsAsync();
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consultants list");
                return StatusCode(500, new { message = "Failed to get consultants list" });
            }
        }

        /// <summary>
        /// Delete a user and all related data (Admin only)
        /// </summary>
        /// <param name="userId">ID of the user to delete</param>
        /// <returns>Success message if user is deleted successfully</returns>
        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);
                return StatusCode(result.Code, new
                {
                    success = result.IsSuccess,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", userId);
                return StatusCode(500, new { message = "Failed to delete user" });
            }
        }
    }
}