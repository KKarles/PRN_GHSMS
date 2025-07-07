using Microsoft.EntityFrameworkCore;
using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public class ConsultantProfileRepo : GenericRepository<ConsultantProfile>, IConsultantProfileRepo
    {
        public ConsultantProfileRepo(GenderHealthcareDBContext context) : base(context)
        {
        }

        public async Task<ConsultantProfile?> GetByConsultantIdAsync(int consultantId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(cp => cp.ConsultantId == consultantId);
        }

        public async Task<ConsultantProfile?> GetByConsultantIdWithUserAsync(int consultantId)
        {
            return await _dbSet
                .Include(cp => cp.Consultant)
                .FirstOrDefaultAsync(cp => cp.ConsultantId == consultantId);
        }

        public async Task<IEnumerable<ConsultantProfile>> GetAllWithUserDetailsAsync()
        {
            return await _dbSet
                .Include(cp => cp.Consultant)
                .ToListAsync();
        }

        public async Task<bool> ExistsByConsultantIdAsync(int consultantId)
        {
            return await _dbSet
                .AnyAsync(cp => cp.ConsultantId == consultantId);
        }

        public async Task<IEnumerable<ConsultantProfile>> GetBySpecializationAsync(string specialization)
        {
            return await _dbSet
                .Include(cp => cp.Consultant)
                .Where(cp => cp.Specialization != null && cp.Specialization.Contains(specialization))
                .ToListAsync();
        }
    }
}