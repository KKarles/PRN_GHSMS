using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class RoleRepo : GenericRepository<Role>, IRoleRepo
    {
        public RoleRepo(GenderHealthcareDbContext context) : base(context)
        {
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        public async Task<IEnumerable<Role>> GetRolesByUserAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.Users.Any(u => u.UserId == userId))
                .ToListAsync();
        }
    }
}