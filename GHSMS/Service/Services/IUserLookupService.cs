using Service.Models;

namespace Service.Services
{
    public interface IUserLookupService
    {
        Task<ResultModel> GetAllUsersAsync(string? role = null, string? search = null, int page = 1, int limit = 10);
    }
}