using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IMenstrualCycleService
    {
        Task<ResultModel> CreateCycleAsync(int userId, CreateMenstrualCycleDto createCycleDto);
        Task<ResultModel> GetCyclesByUserAsync(int userId);
        Task<ResultModel> GetCycleByIdAsync(int cycleId);
        Task<ResultModel> GetUsersNeedingNotificationsAsync();
        Task<ResultModel> UpdateCycleAsync(int cycleId, int userId, UpdateMenstrualCycleDto updateCycleDto);
        Task<ResultModel> DeleteCycleAsync(int cycleId, int userId);
        Task<ResultModel> GetLatestCycleAsync(int userId);
        Task<ResultModel> GetCyclePredictionsAsync(int userId);
        Task<ResultModel> GetActiveCyclesAsync();
        Task<ResultModel> GetCyclesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    }
}