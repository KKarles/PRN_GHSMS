using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;
using System.Security.Cryptography;
using System.Text;

namespace Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IRoleRepo _roleRepo;

        public UserService(IUserRepo userRepo, IRoleRepo roleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
        }

        public async Task<ResultModel> RegisterAsync(UserRegistrationDto registrationDto)
        {
            try
            {
                // Check if email already exists
                if (await _userRepo.EmailExistsAsync(registrationDto.Email))
                {
                    return ResultModel.Conflict("Email already exists");
                }

                // Hash password
                var passwordHash = HashPassword(registrationDto.Password);

                // Create user
                var user = new User
                {
                    FirstName = registrationDto.FirstName,
                    LastName = registrationDto.LastName,
                    Email = registrationDto.Email,
                    PasswordHash = passwordHash,
                    PhoneNumber = registrationDto.PhoneNumber,
                    DateOfBirth = registrationDto.DateOfBirth.HasValue ? DateOnly.FromDateTime(registrationDto.DateOfBirth.Value) : null,
                    Sex = registrationDto.Sex,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepo.CreateAsync(user);

                // Assign Customer role by default
                var customerRole = await _roleRepo.GetByNameAsync("Customer");
                if (customerRole != null)
                {
                    createdUser.Roles.Add(customerRole);
                    await _userRepo.SaveAsync();
                }

                var userProfile = MapToUserProfileDto(createdUser);
                return ResultModel.Created(userProfile, "User registered successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Registration failed: {ex.Message}");
            }
        }

        public async Task<ResultModel> LoginAsync(UserLoginDto loginDto)
        {
            try
            {
                var user = await _userRepo.GetByEmailWithRolesAsync(loginDto.Email);
                if (user == null)
                {
                    return ResultModel.Unauthorized("Invalid email or password");
                }

                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return ResultModel.Unauthorized("Invalid email or password");
                }

                var userProfile = MapToUserProfileDto(user);
                return ResultModel.Success(userProfile, "Login successful");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Login failed: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _userRepo.GetUserWithRolesAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                var userProfile = MapToUserProfileDto(user);
                return ResultModel.Success(userProfile);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get user profile: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateUserProfileAsync(int userId, UserUpdateDto updateDto)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateDto.FirstName))
                    user.FirstName = updateDto.FirstName;
                
                if (!string.IsNullOrEmpty(updateDto.LastName))
                    user.LastName = updateDto.LastName;
                
                if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                    user.PhoneNumber = updateDto.PhoneNumber;
                
                if (updateDto.DateOfBirth.HasValue)
                    user.DateOfBirth = DateOnly.FromDateTime(updateDto.DateOfBirth.Value);
                
                if (!string.IsNullOrEmpty(updateDto.Sex))
                    user.Sex = updateDto.Sex;
                
                if (updateDto.WantsCycleNotifications.HasValue)
                    user.WantsCycleNotifications = updateDto.WantsCycleNotifications.Value;
                
                if (updateDto.PillReminderTime.HasValue)
                    user.PillReminderTime = TimeOnly.FromTimeSpan(updateDto.PillReminderTime.Value);

                user.UpdatedAt = DateTime.UtcNow;

                await _userRepo.UpdateAsync(user);
                
                var userProfile = MapToUserProfileDto(user);
                return ResultModel.Success(userProfile, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update profile: {ex.Message}");
            }
        }

        public async Task<ResultModel> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return ResultModel.BadRequest("Current password is incorrect");
                }

                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepo.UpdateAsync(user);
                return ResultModel.Success(null, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to change password: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepo.GetAllAsync(u => u.Roles);
                var userProfiles = users.Select(MapToUserProfileDto).ToList();
                return ResultModel.Success(userProfiles);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get users: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                var users = await _userRepo.GetUsersByRoleAsync(roleName);
                var userProfiles = users.Select(MapToUserProfileDto).ToList();
                return ResultModel.Success(userProfiles);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get users by role: {ex.Message}");
            }
        }

        public async Task<ResultModel> AssignRoleToUserAsync(int userId, string roleName)
        {
            try
            {
                var user = await _userRepo.GetUserWithRolesAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                var role = await _roleRepo.GetByNameAsync(roleName);
                if (role == null)
                {
                    return ResultModel.NotFound("Role not found");
                }

                if (user.Roles.Any(r => r.RoleName == roleName))
                {
                    return ResultModel.Conflict("User already has this role");
                }

                user.Roles.Add(role);
                await _userRepo.SaveAsync();

                return ResultModel.Success(null, "Role assigned successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to assign role: {ex.Message}");
            }
        }

        public async Task<ResultModel> RemoveRoleFromUserAsync(int userId, string roleName)
        {
            try
            {
                var user = await _userRepo.GetUserWithRolesAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                var role = user.Roles.FirstOrDefault(r => r.RoleName == roleName);
                if (role == null)
                {
                    return ResultModel.NotFound("User does not have this role");
                }

                user.Roles.Remove(role);
                await _userRepo.SaveAsync();

                return ResultModel.Success(null, "Role removed successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to remove role: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateNotificationPreferencesAsync(int userId, bool wantsCycleNotifications, TimeSpan? pillReminderTime)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                user.WantsCycleNotifications = wantsCycleNotifications;
                user.PillReminderTime = pillReminderTime.HasValue ? TimeOnly.FromTimeSpan(pillReminderTime.Value) : null;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepo.UpdateAsync(user);
                return ResultModel.Success(null, "Notification preferences updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update notification preferences: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetUsersWithCycleNotificationsAsync()
        {
            try
            {
                var users = await _userRepo.GetUsersWithCycleNotificationsAsync();
                var userProfiles = users.Select(MapToUserProfileDto).ToList();
                return ResultModel.Success(userProfiles);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get users with cycle notifications: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetUsersWithPillRemindersAsync(TimeSpan currentTime)
        {
            try
            {
                var timeOnly = TimeOnly.FromTimeSpan(currentTime);
                var users = await _userRepo.GetUsersWithPillRemindersAsync(timeOnly);
                var userProfiles = users.Select(MapToUserProfileDto).ToList();
                return ResultModel.Success(userProfiles);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get users with pill reminders: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetEmployeesAsync()
        {
            try
            {
                var staffUsers = await _userRepo.GetUsersByRoleAsync("Staff");
                var consultantUsers = await _userRepo.GetUsersByRoleAsync("Consultant");
                
                var employees = staffUsers.Concat(consultantUsers).Distinct().ToList();
                var employeeProfiles = employees.Select(MapToUserProfileDto).ToList();
                
                return ResultModel.Success(employeeProfiles, "Employees retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get employees: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetStaffAsync()
        {
            try
            {
                var users = await _userRepo.GetUsersByRoleAsync("Staff");
                var userProfiles = users.Select(MapToUserProfileDto).ToList();
                return ResultModel.Success(userProfiles, "Staff users retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get staff users: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetConsultantsAsync()
        {
            try
            {
                var users = await _userRepo.GetUsersByRoleAsync("Consultant");
                var userProfiles = users.Select(MapToUserProfileDto).ToList();
                return ResultModel.Success(userProfiles, "Consultant users retrieved successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get consultant users: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _userRepo.GetUserWithRolesAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                // Check if user is admin - prevent deleting admin users
                if (user.Roles.Any(r => r.RoleName == "Admin"))
                {
                    return ResultModel.BadRequest("Cannot delete admin users");
                }

                // Get user with all related data to check dependencies
                var userWithDetails = await _userRepo.GetByIdAsync(userId);
                if (userWithDetails == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                // Note: Due to EF Core cascade delete configuration, related data should be handled automatically
                // However, we should explicitly handle critical relationships to ensure data integrity

                // Delete the user and all related data
                var deleteResult = await _userRepo.DeleteUserAsync(userId);
                
                if (!deleteResult)
                {
                    return ResultModel.InternalServerError("Failed to delete user from database");
                }

                return ResultModel.Success(null, $"User '{user.FirstName} {user.LastName}' and all related data deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to delete user: {ex.Message}");
            }
        }

        private UserProfileDto MapToUserProfileDto(User user)
        {
            return new UserProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                Sex = user.Sex,
                WantsCycleNotifications = user.WantsCycleNotifications,
                PillReminderTime = user.PillReminderTime?.ToTimeSpan(),
                Roles = user.Roles?.Select(r => r.RoleName).ToList() ?? new List<string>()
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}