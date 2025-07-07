using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public interface ITestResultService
    {
        Task<ResultModel> CreateTestResultAsync(int issuedByUserId, CreateTestResultDto createResultDto);
        Task<ResultModel> GetTestResultByIdAsync(int resultId);
        Task<ResultModel> GetTestResultByBookingIdAsync(int bookingId);
        Task<ResultModel> GetTestResultsByCustomerAsync(int customerId);
        Task<ResultModel> UpdateTestResultAsync(int resultId, UpdateTestResultDto updateResultDto);
        Task<ResultModel> GetTestResultsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ResultModel> DeleteTestResultAsync(int resultId);
    }
}