using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface IUserRepo : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailWithRolesAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetUserWithRolesAsync(int userId);
        Task<IEnumerable<User>> GetUsersWithCycleNotificationsAsync();
        Task<IEnumerable<User>> GetUsersWithPillRemindersAsync(TimeOnly currentTime);
        Task<bool> DeleteUserAsync(int userId);
    }
}