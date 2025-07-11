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
    }
}