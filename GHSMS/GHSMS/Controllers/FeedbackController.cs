using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Services;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateFeedbackDto dto)
        {
            var result = await _feedbackService.CreateFeedbackAsync(dto);
            return StatusCode(result.Code, result);
        }

        [HttpGet("user/{userId}/service/{serviceId}")]
        public async Task<IActionResult> GetByUserAndService(int userId, int serviceId)
        {
            var feedback = await _feedbackService.GetFeedbackByUserAndServiceAsync(userId, serviceId);
            if (feedback == null) return NotFound();
            return Ok(feedback);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
                return NotFound();

            return Ok(feedback);
        }

        [HttpPut("{feedbackId}")]
        public async Task<IActionResult> Update(int feedbackId, [FromBody] UpdateFeedbackDto dto)
        {
            var success = await _feedbackService.UpdateFeedbackAsync(feedbackId, dto.Rating, dto.Comment);
            if (!success) return NotFound();
            return Ok(new { message = "Feedback updated successfully." });
        }

        public class UpdateFeedbackDto
        {
            public byte Rating { get; set; }
            public string Comment { get; set; }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var success = await _feedbackService.DeleteFeedbackAsync(id);
            if (!success)
                return NotFound(new { success = false, message = "Feedback not found." });

            return Ok(new { success = true });
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetFeedbacksByService(int serviceId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByServiceAsync(serviceId);
            return Ok(feedbacks);
        }



    }

}
