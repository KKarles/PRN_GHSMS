using Repository.Models;
using Service.DTOs;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public interface IFeedbackService
    {
        Task<ResultModel> CreateFeedbackAsync(CreateFeedbackDto dto);
        Task<Feedback?> GetFeedbackByUserAndServiceAsync(int userId, int serviceId);
        Task<bool> UpdateFeedbackAsync(int feedbackId, byte rating, string comment);
        Task<Feedback> GetFeedbackByIdAsync(int id);
        Task<bool> DeleteFeedbackAsync(int feedbackId);
        Task<List<Feedback>> GetFeedbacksByServiceAsync(int serviceId);


    }
}
