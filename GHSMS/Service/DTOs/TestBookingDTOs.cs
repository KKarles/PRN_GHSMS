using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class CreateTestBookingDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime AppointmentTime { get; set; }
    }

    public class TestBookingDto
    {
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public DateTime? AppointmentTime { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public DateTime BookedAt { get; set; }
        public DateTime? ResultDate { get; set; }
    }

    public class UpdateBookingStatusDto
    {
        [Required]
        public string BookingStatus { get; set; } = string.Empty;

        public bool? IsPaid { get; set; }

        public DateTime? ResultDate { get; set; }
    }

    public class BookingStatsDto
    {
        public int TotalBookings { get; set; }
        public int PaidBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, int> BookingsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> BookingsByService { get; set; } = new Dictionary<string, int>();
    }
}