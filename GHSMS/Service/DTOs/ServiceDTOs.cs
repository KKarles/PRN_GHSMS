using System.ComponentModel.DataAnnotations;

namespace Service.DTOs
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public List<AnalyteDto> Analytes { get; set; } = new List<AnalyteDto>();
    }

    public class AnalyteDto
    {
        public int AnalyteId { get; set; }
        public string AnalyteName { get; set; } = string.Empty;
        public string? DefaultUnit { get; set; }
        public string? DefaultReferenceRange { get; set; }
    }

    public class CreateServiceDto
    {
        [Required]
        [MaxLength(255)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string ServiceType { get; set; } = string.Empty;

        public List<int> AnalyteIds { get; set; } = new List<int>();
    }

    public class UpdateServiceDto
    {
        [MaxLength(255)]
        public string? ServiceName { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        public string? ServiceType { get; set; }

        public List<int>? AnalyteIds { get; set; }
    }
}