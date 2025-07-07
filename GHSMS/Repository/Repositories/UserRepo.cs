using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class UserRepo : GenericRepository<User>, IUserRepo
    {
        public UserRepo(GenderHealthcareDBContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailWithRolesAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await _dbSet
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.RoleName == roleName))
                .ToListAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserWithRolesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<IEnumerable<User>> GetUsersWithCycleNotificationsAsync()
        {
            return await _dbSet
                .Where(u => u.WantsCycleNotifications)
                .Include(u => u.MenstrualCycles)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersWithPillRemindersAsync(TimeOnly currentTime)
        {
            // Get all users with pill reminders enabled - we'll filter by time in the service layer
            return await _dbSet
                .Where(u => u.WantsCycleNotifications && u.PillReminderTime != null)
                .ToListAsync();
        }
    }
}