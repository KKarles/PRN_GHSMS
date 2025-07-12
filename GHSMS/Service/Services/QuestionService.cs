using Repository.Models;
using Repository.Base;
using Service.DTOs;
using Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IGenericRepository<Question> _questionRepo;
        private readonly IGenericRepository<Answer> _answerRepo;
        private readonly IGenericRepository<User> _userRepo;

        public QuestionService(
            IGenericRepository<Question> questionRepo,
            IGenericRepository<Answer> answerRepo,
            IGenericRepository<User> userRepo)
        {
            _questionRepo = questionRepo;
            _answerRepo = answerRepo;
            _userRepo = userRepo;
        }

        public async Task<ResultModel> CreateQuestionAsync(int customerId, CreateQuestionDto dto)
        {
            var user = await _userRepo.GetByIdAsync(customerId);
            if (user == null)
                return ResultModel.NotFound("Customer not found");

            var question = new Question
            {
                CustomerId = customerId,
                Title = dto.Title,
                QuestionText = dto.QuestionText,
                IsAnonymous = dto.IsAnonymous,
            };
            await _questionRepo.CreateAsync(question);
            await _questionRepo.SaveAsync();
            return ResultModel.Created(question.QuestionId, "Question posted successfully");
        }

        public async Task<ResultModel> GetQuestionsAsync()
        {
            var questions = await _questionRepo.GetAllAsync(q => q.Customer, q => q.Answers);
            var result = questions.Select(q => new QuestionDto
            {
                QuestionId = q.QuestionId,
                CustomerId = q.CustomerId,
                Title = q.Title,
                QuestionText = q.QuestionText,
                IsAnonymous = q.IsAnonymous ?? false,
                CustomerName = (q.IsAnonymous ?? false) ? "Anonymous" : $"{q.Customer?.FirstName} {q.Customer?.LastName}",
                CreatedAt = DateTime.Now, // Replace with actual CreatedAt if available
                Answers = q.Answers.Select(a => new AnswerDto
                {
                    AnswerId = a.AnswerId,
                    QuestionId = a.QuestionId,
                    ConsultantId = a.ConsultantId,
                    ConsultantName = $"{a.Consultant?.FirstName} {a.Consultant?.LastName}",
                    AnswerText = a.AnswerText,
                    CreatedAt = DateTime.Now // Replace with actual CreatedAt if available
                }).ToList()
            }).ToList();
            return ResultModel.Success(result);
        }

        public async Task<ResultModel> GetQuestionByIdAsync(int questionId)
        {
            var q = await _questionRepo.GetByIdAsync(questionId, x => x.Customer, x => x.Answers);
            if (q == null)
                return ResultModel.NotFound("Question not found");
            var dto = new QuestionDto
            {
                QuestionId = q.QuestionId,
                CustomerId = q.CustomerId,
                Title = q.Title,
                QuestionText = q.QuestionText,
                IsAnonymous = q.IsAnonymous ?? false,
                CustomerName = (q.IsAnonymous ?? false) ? "Anonymous" : $"{q.Customer?.FirstName} {q.Customer?.LastName}",
                CreatedAt = DateTime.Now, // Replace with actual CreatedAt if available
                Answers = q.Answers.Select(a => new AnswerDto
                {
                    AnswerId = a.AnswerId,
                    QuestionId = a.QuestionId,
                    ConsultantId = a.ConsultantId,
                    ConsultantName = $"{a.Consultant?.FirstName} {a.Consultant?.LastName}",
                    AnswerText = a.AnswerText,
                    CreatedAt = DateTime.Now // Replace with actual CreatedAt if available
                }).ToList()
            };
            return ResultModel.Success(dto);
        }

        public async Task<ResultModel> DeleteQuestionAsync(int questionId, int userId, bool isAdminOrManager)
        {
            var question = await _questionRepo.GetByIdAsync(questionId, q => q.Answers);
            if (question == null)
                return ResultModel.NotFound("Question not found");
            if (!isAdminOrManager && question.CustomerId != userId)
                return ResultModel.Forbidden("You can only delete your own questions");
            // Delete all answers first
            if (question.Answers != null && question.Answers.Any())
            {
                foreach (var answer in question.Answers.ToList())
                {
                    await _answerRepo.RemoveAsync(answer);
                }
            }
            await _questionRepo.RemoveAsync(question);
            await _questionRepo.SaveAsync();
            return ResultModel.Success(null, "Question deleted");
        }

        public async Task<ResultModel> CreateAnswerAsync(int consultantId, CreateAnswerDto dto)
        {
            var user = await _userRepo.GetByIdAsync(consultantId);
            if (user == null)
                return ResultModel.NotFound("Consultant not found");
            var question = await _questionRepo.GetByIdAsync(dto.QuestionId);
            if (question == null)
                return ResultModel.NotFound("Question not found");
            var answer = new Answer
            {
                QuestionId = dto.QuestionId,
                ConsultantId = consultantId,
                AnswerText = dto.AnswerText,
            };
            await _answerRepo.CreateAsync(answer);
            await _answerRepo.SaveAsync();
            return ResultModel.Created(answer.AnswerId, "Answer posted successfully");
        }

        public async Task<ResultModel> DeleteAnswerAsync(int answerId, int userId, bool isAdminOrManager)
        {
            var answer = await _answerRepo.GetByIdAsync(answerId);
            if (answer == null)
                return ResultModel.NotFound("Answer not found");
            if (!isAdminOrManager && answer.ConsultantId != userId)
                return ResultModel.Forbidden("You can only delete your own answers");
            await _answerRepo.RemoveAsync(answer);
            await _answerRepo.SaveAsync();
            return ResultModel.Success(null, "Answer deleted");
        }

        public async Task<ResultModel> GetAnswersByQuestionIdAsync(int questionId)
        {
            var question = await _questionRepo.GetByIdAsync(questionId, q => q.Answers);
            if (question == null)
                return ResultModel.NotFound("Question not found");
            var answers = question.Answers.Select(a => new AnswerDto
            {
                AnswerId = a.AnswerId,
                QuestionId = a.QuestionId,
                ConsultantId = a.ConsultantId,
                ConsultantName = $"{a.Consultant?.FirstName} {a.Consultant?.LastName}",
                AnswerText = a.AnswerText,
                CreatedAt = DateTime.Now // Replace with actual CreatedAt if available
            }).ToList();
            return ResultModel.Success(answers);
        }
    }
}
