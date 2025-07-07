using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Services;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceCatalogController : ControllerBase
    {
        private readonly IServiceCatalogService _serviceCatalogService;

        public ServiceCatalogController(IServiceCatalogService serviceCatalogService)
        {
            _serviceCatalogService = serviceCatalogService;
        }

        /// <summary>
        /// Get all available services (Public access for browsing)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var result = await _serviceCatalogService.GetAllServicesAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get service by ID with analytes (Public access)
        /// </summary>
        [HttpGet("{serviceId}")]
        public async Task<IActionResult> GetServiceById(int serviceId)
        {
            var result = await _serviceCatalogService.GetServiceByIdAsync(serviceId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get services by type (e.g., "Test", "Package") (Public access)
        /// </summary>
        [HttpGet("type/{serviceType}")]
        public async Task<IActionResult> GetServicesByType(string serviceType)
        {
            var result = await _serviceCatalogService.GetServicesByTypeAsync(serviceType);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get services by price range (Public access)
        /// </summary>
        [HttpGet("price-range")]
        public async Task<IActionResult> GetServicesByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            var result = await _serviceCatalogService.GetServicesByPriceRangeAsync(minPrice, maxPrice);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Get all analytes (Public access for information)
        /// </summary>
        [HttpGet("analytes")]
        public async Task<IActionResult> GetAllAnalytes()
        {
            var result = await _serviceCatalogService.GetAllAnalytesAsync();
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Create new service (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto createServiceDto)
        {
            var result = await _serviceCatalogService.CreateServiceAsync(createServiceDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Update service (Admin only)
        /// </summary>
        [HttpPut("{serviceId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int serviceId, [FromBody] UpdateServiceDto updateServiceDto)
        {
            var result = await _serviceCatalogService.UpdateServiceAsync(serviceId, updateServiceDto);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message, 
                data = result.Data 
            });
        }

        /// <summary>
        /// Delete service (Admin only)
        /// </summary>
        [HttpDelete("{serviceId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int serviceId)
        {
            var result = await _serviceCatalogService.DeleteServiceAsync(serviceId);
            return StatusCode(result.Code, new { 
                success = result.IsSuccess, 
                message = result.Message 
            });
        }
    }
}