using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class ServiceCatalogService : IServiceCatalogService
    {
        private readonly IServiceRepo _serviceRepo;
        private readonly IGenericRepository<Analyte> _analyteRepo;

        public ServiceCatalogService(IServiceRepo serviceRepo, IGenericRepository<Analyte> analyteRepo)
        {
            _serviceRepo = serviceRepo;
            _analyteRepo = analyteRepo;
        }

        public async Task<ResultModel> GetAllServicesAsync()
        {
            try
            {
                var services = await _serviceRepo.GetAllWithAnalytesAsync();
                var serviceDtos = services.Select(MapToServiceDto).ToList();
                return ResultModel.Success(serviceDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get services: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetServiceByIdAsync(int serviceId)
        {
            try
            {
                var service = await _serviceRepo.GetByIdWithAnalytesAsync(serviceId);
                if (service == null)
                {
                    return ResultModel.NotFound("Service not found");
                }

                var serviceDto = MapToServiceDto(service);
                return ResultModel.Success(serviceDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get service: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetServicesByTypeAsync(string serviceType)
        {
            try
            {
                var services = await _serviceRepo.GetByServiceTypeAsync(serviceType);
                var serviceDtos = services.Select(MapToServiceDto).ToList();
                return ResultModel.Success(serviceDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get services by type: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetServicesByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                var services = await _serviceRepo.GetServicesByPriceRangeAsync(minPrice, maxPrice);
                var serviceDtos = services.Select(MapToServiceDto).ToList();
                return ResultModel.Success(serviceDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get services by price range: {ex.Message}");
            }
        }

        public async Task<ResultModel> CreateServiceAsync(CreateServiceDto createServiceDto)
        {
            try
            {
                // Check if service name already exists
                var existingService = await _serviceRepo.GetByNameAsync(createServiceDto.ServiceName);
                if (existingService != null)
                {
                    return ResultModel.Conflict("Service with this name already exists");
                }

                // Get analytes
                var analytes = new List<Analyte>();
                foreach (var analyteId in createServiceDto.AnalyteIds)
                {
                    var analyte = await _analyteRepo.GetByIdAsync(analyteId);
                    if (analyte != null)
                    {
                        analytes.Add(analyte);
                    }
                }

                var service = new Repository.Models.Service
                {
                    ServiceName = createServiceDto.ServiceName,
                    Description = createServiceDto.Description,
                    Price = createServiceDto.Price,
                    ServiceType = createServiceDto.ServiceType,
                    Analytes = analytes
                };

                var createdService = await _serviceRepo.CreateAsync(service);
                var serviceDto = MapToServiceDto(createdService);
                return ResultModel.Created(serviceDto, "Service created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to create service: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateServiceAsync(int serviceId, UpdateServiceDto updateServiceDto)
        {
            try
            {
                var service = await _serviceRepo.GetByIdWithAnalytesAsync(serviceId);
                if (service == null)
                {
                    return ResultModel.NotFound("Service not found");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateServiceDto.ServiceName))
                    service.ServiceName = updateServiceDto.ServiceName;

                if (!string.IsNullOrEmpty(updateServiceDto.Description))
                    service.Description = updateServiceDto.Description;

                if (updateServiceDto.Price.HasValue)
                    service.Price = updateServiceDto.Price.Value;

                if (!string.IsNullOrEmpty(updateServiceDto.ServiceType))
                    service.ServiceType = updateServiceDto.ServiceType;

                // Update analytes if provided
                if (updateServiceDto.AnalyteIds != null)
                {
                    service.Analytes.Clear();
                    foreach (var analyteId in updateServiceDto.AnalyteIds)
                    {
                        var analyte = await _analyteRepo.GetByIdAsync(analyteId);
                        if (analyte != null)
                        {
                            service.Analytes.Add(analyte);
                        }
                    }
                }

                await _serviceRepo.UpdateAsync(service);
                var serviceDto = MapToServiceDto(service);
                return ResultModel.Success(serviceDto, "Service updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update service: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteServiceAsync(int serviceId)
        {
            try
            {
                var service = await _serviceRepo.GetByIdAsync(serviceId);
                if (service == null)
                {
                    return ResultModel.NotFound("Service not found");
                }

                await _serviceRepo.RemoveAsync(service);
                return ResultModel.Success(null, "Service deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to delete service: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetAllAnalytesAsync()
        {
            try
            {
                var analytes = await _analyteRepo.GetAllAsync();
                var analyteDtos = analytes.Select(MapToAnalyteDto).ToList();
                return ResultModel.Success(analyteDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get analytes: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetServiceWithAnalytesAsync(int serviceId)
        {
            try
            {
                var service = await _serviceRepo.GetByIdWithAnalytesAsync(serviceId);
                if (service == null)
                {
                    return ResultModel.NotFound("Service not found");
                }

                var serviceDto = MapToServiceDto(service);
                return ResultModel.Success(serviceDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get service with analytes: {ex.Message}");
            }
        }

        private ServiceDto MapToServiceDto(Repository.Models.Service service)
        {
            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                ServiceType = service.ServiceType,
                Analytes = service.Analytes?.Select(MapToAnalyteDto).ToList() ?? new List<AnalyteDto>()
            };
        }

        private AnalyteDto MapToAnalyteDto(Analyte analyte)
        {
            return new AnalyteDto
            {
                AnalyteId = analyte.AnalyteId,
                AnalyteName = analyte.AnalyteName,
                DefaultUnit = analyte.DefaultUnit,
                DefaultReferenceRange = analyte.DefaultReferenceRange
            };
        }
    }
}