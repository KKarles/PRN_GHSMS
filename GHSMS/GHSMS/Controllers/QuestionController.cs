using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Services;
using System.Security.Claims;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Post a new question (Customer only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PostQuestion([FromBody] CreateQuestionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int customerId))
                return Unauthorized(new { message = "Invalid token" });
            var result = await _questionService.CreateQuestionAsync(customerId, dto);
            return StatusCode(result.Code, new { success = result.IsSuccess, message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Get all questions (Public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuestions()
        {
            var result = await _questionService.GetQuestionsAsync();
            return StatusCode(result.Code, new { success = result.IsSuccess, message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Get question by ID (Public)
        /// </summary>
        [HttpGet("{questionId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var result = await _questionService.GetQuestionByIdAsync(questionId);
            return StatusCode(result.Code, new { success = result.IsSuccess, message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Get all answers for a specific question by questionId (Public)
        /// </summary>
        [HttpGet("{questionId}/answers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnswersByQuestionId(int questionId)
        {
            var result = await _questionService.GetAnswersByQuestionIdAsync(questionId);
            return StatusCode(result.Code, new { success = result.IsSuccess, message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Delete a question (Customer can delete own, Admin/Manager can delete any)
        /// </summary>
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Invalid token" });
            bool isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
            var result = await _questionService.DeleteQuestionAsync(questionId, userId, isAdminOrManager);
            return StatusCode(result.Code, new { success = result.IsSuccess, message = result.Message });
        }

        /// <summary>
        /// Post an answer to a question (Consultant only)
        /// </summary>
        [HttpPost("answer")]
        [Authorize(Roles = "Consultant")]
        public async Task<IActionResult> PostAnswer([FromBody] CreateAnswerDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int consultantId))
                return Unauthorized(new { message = "Invalid token" });
            var result = await _questionService.CreateAnswerAsync(consultantId, dto);
            return StatusCode(result.Code, new { success = result.IsSuccess, message = result.Message, data = result.Data });
        }

        /// <summary>
        /// Delete an answer (Consultant can delete own, Admin/Manager can delete any)
        /// </summary>
        [HttpDelete("answer/{answerId}")]
        public async Task<IActionResult> DeleteAnswer(int answerId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { message = "Invalid token" });
            bool isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
            var result = await _questionService.DeleteAnswerAsync(answerId, userId, isAdminOrManager);
            return StatusCode(result.Code, new { success = result.IsSuccess, message = result.Message });
        }
    }
}
