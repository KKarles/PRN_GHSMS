using Repository.Base;
using Repository.Models;
using Repository.Repositories.Repository.Repositories;
using Service.DTOs;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepo _feedbackRepo;
        private readonly IGenericRepository<User> _userRepo;
        private readonly IGenericRepository<Repository.Models.Service> _serviceRepo;

        public FeedbackService(IFeedbackRepo feedbackRepo,
            IGenericRepository<User> userRepo,
            IGenericRepository<Repository.Models.Service> serviceRepo)
        {
            _feedbackRepo = feedbackRepo;
            _userRepo = userRepo;
            _serviceRepo = serviceRepo;
        }

        public async Task<ResultModel> CreateFeedbackAsync(CreateFeedbackDto dto)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(dto.UserId);
                var service = await _serviceRepo.GetByIdAsync(dto.RelatedServiceId);

                if (user == null || service == null)
                    return ResultModel.NotFound("User or Service not found");

                var newFeedback = new Feedback
                {
                    UserId = dto.UserId,
                    RelatedServiceId = dto.RelatedServiceId,
                    RelatedAppointmentId = null,
                    Rating = dto.Rating,
                    Comment = dto.Comment
                };

                var created = await _feedbackRepo.CreateAsync(newFeedback);

                return ResultModel.Created(created, "Feedback created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Error creating feedback: {ex.Message}");
            }
        }

        public async Task<Feedback?> GetFeedbackByUserAndServiceAsync(int userId, int serviceId)
        {
            return await _feedbackRepo.GetFeedbackByUserAndServiceAsync(userId, serviceId);
        }

        public async Task<bool> UpdateFeedbackAsync(int feedbackId, byte rating, string comment)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
            if (feedback == null) return false;

            feedback.Rating = rating;
            feedback.Comment = comment;

            await _feedbackRepo.UpdateAsync(feedback);
            return true;
        }

        public async Task<Feedback> GetFeedbackByIdAsync(int id)
        {
            return await _feedbackRepo.GetByIdAsync(id);
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
            if (feedback == null)
                return false;

            await _feedbackRepo.RemoveAsync(feedback);
            return true;
        }

        public async Task<List<Feedback>> GetFeedbacksByServiceAsync(int serviceId)
        {
            var feedbacks = await _feedbackRepo.FindAsync(f => f.RelatedServiceId == serviceId);
            return feedbacks.ToList();
        }


    }
}
