using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface IStaffQualificationService
    {
        Task<ResultModel> GetStaffQualificationAsync(int staffId);
        Task<ResultModel> CreateStaffQualificationAsync(int staffId, CreateStaffQualificationDto createDto);
        Task<ResultModel> UpdateStaffQualificationAsync(int staffId, UpdateStaffQualificationDto updateDto);
        Task<ResultModel> DeleteStaffQualificationAsync(int staffId);
        Task<ResultModel> GetAllStaffQualificationsAsync();
        Task<ResultModel> GetStaffBySpecializationAsync(string specialization);
        Task<ResultModel> GetStaffWithoutQualificationsAsync();
    }
}