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
                .Include(u => u.ConsultantProfile)
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

        public async Task<User?> GetUserProfileAsync(int userId)
        {
            return await _dbSet
                .Where(u => u.UserId == userId)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    DateOfBirth = u.DateOfBirth,
                    Sex = u.Sex,
                    WantsCycleNotifications = u.WantsCycleNotifications,
                    PillReminderTime = u.PillReminderTime,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Roles = u.Roles
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                // Get user with all related data for cascade delete
                var user = await _dbSet
                    .Include(u => u.Roles)
                    .Include(u => u.Answers)
                    .Include(u => u.AppointmentConsultants)
                    .Include(u => u.AppointmentCustomers)
                    .Include(u => u.BlogPosts)
                    .Include(u => u.ConsultantProfile)
                    .Include(u => u.Feedbacks)
                    .Include(u => u.MenstrualCycles)
                    .Include(u => u.Questions)
                    .Include(u => u.Schedules)
                    .Include(u => u.TestBookings)
                    .Include(u => u.TestResults)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return false;
                }

                // Remove the user - EF Core will handle cascade deletes
                _dbSet.Remove(user);
                var result = await _context.SaveChangesAsync();
                
                return result > 0;
            }
            catch (Exception)
            {
                // Log the exception if needed
                return false;
            }
        }
    }
}