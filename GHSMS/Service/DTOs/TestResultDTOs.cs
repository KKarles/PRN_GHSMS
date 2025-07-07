using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class CreateTestResultDto
    {
        [Required]
        public int BookingId { get; set; }

        public string? Notes { get; set; }

        [Required]
        public List<TestResultDetailDto> ResultDetails { get; set; } = new List<TestResultDetailDto>();
    }

    public class TestResultDetailDto
    {
        [Required]
        public string AnalyteName { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        public string? Unit { get; set; }

        public string? ReferenceRange { get; set; }

        public string? Flag { get; set; }
    }

    public class TestResultDto
    {
        public int ResultId { get; set; }
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string IssuedByName { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public List<TestResultDetailDto> ResultDetails { get; set; } = new List<TestResultDetailDto>();
    }

    public class UpdateTestResultDto
    {
        public string? Notes { get; set; }
        public List<TestResultDetailDto>? ResultDetails { get; set; }
    }
}