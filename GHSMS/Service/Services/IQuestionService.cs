using Service.DTOs;
using Service.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Service.Services
{
    public interface IQuestionService
    {
        Task<ResultModel> CreateQuestionAsync(int customerId, CreateQuestionDto dto);
        Task<ResultModel> GetQuestionsAsync();
        Task<ResultModel> GetQuestionByIdAsync(int questionId);
        Task<ResultModel> DeleteQuestionAsync(int questionId, int userId, bool isAdminOrManager);
        Task<ResultModel> CreateAnswerAsync(int consultantId, CreateAnswerDto dto);
        Task<ResultModel> DeleteAnswerAsync(int answerId, int userId, bool isAdminOrManager);
        Task<ResultModel> GetAnswersByQuestionIdAsync(int questionId);
    }
}
