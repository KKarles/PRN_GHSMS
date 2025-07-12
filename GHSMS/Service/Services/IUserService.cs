using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IUserService
    {
        Task<ResultModel> RegisterAsync(UserRegistrationDto registrationDto);
        Task<ResultModel> LoginAsync(UserLoginDto loginDto);
        Task<ResultModel> GetUserProfileAsync(int userId);
        Task<ResultModel> UpdateUserProfileAsync(int userId, UserUpdateDto updateDto);
        Task<ResultModel> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<ResultModel> GetAllUsersAsync();
        Task<ResultModel> GetUsersByRoleAsync(string roleName);
        Task<ResultModel> AssignRoleToUserAsync(int userId, string roleName);
        Task<ResultModel> RemoveRoleFromUserAsync(int userId, string roleName);
        Task<ResultModel> UpdateNotificationPreferencesAsync(int userId, bool wantsCycleNotifications, TimeSpan? pillReminderTime);
        Task<ResultModel> GetUsersWithCycleNotificationsAsync();
        Task<ResultModel> GetUsersWithPillRemindersAsync(TimeSpan currentTime);
        Task<ResultModel> GetEmployeesAsync();
        Task<ResultModel> GetStaffAsync();
        Task<ResultModel> GetConsultantsAsync();
        Task<ResultModel> DeleteUserAsync(int userId);
    }
}