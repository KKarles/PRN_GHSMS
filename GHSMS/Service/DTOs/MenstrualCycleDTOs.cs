using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class CreateMenstrualCycleDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(21, 35)]
        public int ExpectedCycleLength { get; set; } = 28;
    }

    public class MenstrualCycleDto
    {
        public int CycleId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ExpectedCycleLength { get; set; }
        public int? ActualCycleLength { get; set; }
        public DateTime? PredictedNextPeriod { get; set; }
        public DateTime? PredictedOvulation { get; set; }
        public DateTime? FertileWindowStart { get; set; }
        public DateTime? FertileWindowEnd { get; set; }
    }

    public class UpdateMenstrualCycleDto
    {
        public DateTime? EndDate { get; set; }

        [Range(21, 35)]
        public int? ExpectedCycleLength { get; set; }
    }

    public class CyclePredictionDto
    {
        public DateTime? NextPeriodDate { get; set; }
        public DateTime? OvulationDate { get; set; }
        public DateTime? FertileWindowStart { get; set; }
        public DateTime? FertileWindowEnd { get; set; }
        public double AverageCycleLength { get; set; }
        public bool HasSufficientData { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}