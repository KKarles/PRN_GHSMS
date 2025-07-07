using GHSMS.Models;
using GHSMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Services;
using System.Security.Claims;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AuthController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var registrationDto = new UserRegistrationDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Sex = request.Sex
            };

            var result = await _userService.RegisterAsync(registrationDto);

            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new AuthResponse
                {
                    Success = false,
                    Message = result.Message
                });
            }

            var userProfile = result.Data as UserProfileDto;
            if (userProfile == null)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Registration failed"
                });
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(
                new Repository.Models.User
                {
                    UserId = userProfile.UserId,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Email = userProfile.Email
                },
                userProfile.Roles
            );

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    UserId = userProfile.UserId,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Email = userProfile.Email,
                    Roles = userProfile.Roles
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var loginDto = new UserLoginDto
            {
                Email = request.Email,
                Password = request.Password
            };

            var result = await _userService.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new AuthResponse
                {
                    Success = false,
                    Message = result.Message
                });
            }

            var userProfile = result.Data as UserProfileDto;
            if (userProfile == null)
            {
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Login failed"
                });
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(
                new Repository.Models.User
                {
                    UserId = userProfile.UserId,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Email = userProfile.Email
                },
                userProfile.Roles
            );

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    UserId = userProfile.UserId,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Email = userProfile.Email,
                    Roles = userProfile.Roles
                }
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _userService.GetUserProfileAsync(userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _userService.GetUserProfileAsync(userId);

            if (!result.IsSuccess)
            {
                return StatusCode(result.Code, new { message = result.Message });
            }

            var userProfile = result.Data as UserProfileDto;
            if (userProfile == null)
            {
                return StatusCode(500, new { message = "Failed to refresh token" });
            }

            // Generate new JWT token
            var token = _jwtService.GenerateToken(
                new Repository.Models.User
                {
                    UserId = userProfile.UserId,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Email = userProfile.Email
                },
                userProfile.Roles
            );

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserInfo
                {
                    UserId = userProfile.UserId,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Email = userProfile.Email,
                    Roles = userProfile.Roles
                }
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // In a real application, you might want to blacklist the token
            // For now, we'll just return a success message
            return Ok(new { message = "Logged out successfully" });
        }
    }
}