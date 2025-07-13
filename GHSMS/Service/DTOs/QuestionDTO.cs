using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public string CustomerName { get; set; } = string.Empty; // "Anonymous" if IsAnonymous
        public DateTime CreatedAt { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }

    public class CreateAnswerDto
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string AnswerText { get; set; } = string.Empty;
    }

    public class AnswerDto
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public int ConsultantId { get; set; }
        public string ConsultantName { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
