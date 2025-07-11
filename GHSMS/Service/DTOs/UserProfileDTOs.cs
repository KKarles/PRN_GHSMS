using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class CustomerProfileDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Sex { get; set; }
        public bool WantsCycleNotifications { get; set; }
        public TimeSpan? PillReminderTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateCustomerProfileDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Sex { get; set; }
    }

    public class NotificationSettingsDto
    {
        [Required]
        public bool WantsCycleNotifications { get; set; }

        public TimeSpan? PillReminderTime { get; set; }
    }

    public class StaffQualificationDto
    {
        public int ConsultantId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Qualifications { get; set; }
        public string? Experience { get; set; }
        public string? Specialization { get; set; }
        public bool HasProfile { get; set; }
    }

    public class CreateStaffQualificationDto
    {
        [Required]
        [MaxLength(500)]
        public string Qualifications { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Experience { get; set; }

        [MaxLength(255)]
        public string? Specialization { get; set; }
    }

    public class UpdateStaffQualificationDto
    {
        [MaxLength(500)]
        public string? Qualifications { get; set; }

        [MaxLength(1000)]
        public string? Experience { get; set; }

        [MaxLength(255)]
        public string? Specialization { get; set; }
    }

    public class CustomerDashboardSummaryDto
    {
        public CustomerProfileDto Profile { get; set; } = new CustomerProfileDto();
        public int TotalBookings { get; set; }
        public int CompletedTests { get; set; }
        public int PendingResults { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastVisit { get; set; }
        public DateTime? NextAppointment { get; set; }
        public List<TestBookingDto> RecentBookings { get; set; } = new List<TestBookingDto>();
        public NotificationSettingsDto NotificationSettings { get; set; } = new NotificationSettingsDto();
    }

    public class CreateQuestionDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string QuestionText { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; }
    }

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