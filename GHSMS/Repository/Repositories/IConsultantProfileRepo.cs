using Repository.Base;
using Repository.Models;

namespace Repository.Repositories
{
    public interface IConsultantProfileRepo : IGenericRepository<ConsultantProfile>
    {
        Task<ConsultantProfile?> GetByConsultantIdAsync(int consultantId);
        Task<ConsultantProfile?> GetByConsultantIdWithUserAsync(int consultantId);
        Task<IEnumerable<ConsultantProfile>> GetAllWithUserDetailsAsync();
        Task<bool> ExistsByConsultantIdAsync(int consultantId);
        Task<IEnumerable<ConsultantProfile>> GetBySpecializationAsync(string specialization);
    }
}