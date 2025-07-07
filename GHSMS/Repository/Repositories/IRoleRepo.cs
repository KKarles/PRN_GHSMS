using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface IRoleRepo : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string roleName);
        Task<IEnumerable<Role>> GetRolesByUserAsync(int userId);
    }
}