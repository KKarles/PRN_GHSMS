using Repository.Base;
using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepo _testResultRepo;
        private readonly ITestBookingRepo _testBookingRepo;
        private readonly IUserRepo _userRepo;
        private readonly IGenericRepository<TestResultDetail> _testResultDetailRepo;

        public TestResultService(
            ITestResultRepo testResultRepo, 
            ITestBookingRepo testBookingRepo, 
            IUserRepo userRepo,
            IGenericRepository<TestResultDetail> testResultDetailRepo)
        {
            _testResultRepo = testResultRepo;
            _testBookingRepo = testBookingRepo;
            _userRepo = userRepo;
            _testResultDetailRepo = testResultDetailRepo;
        }

        public async Task<ResultModel> CreateTestResultAsync(int issuedByUserId, CreateTestResultDto createResultDto)
        {
            try
            {
                // Validate booking exists
                var booking = await _testBookingRepo.GetBookingWithDetailsAsync(createResultDto.BookingId);
                if (booking == null)
                {
                    return ResultModel.NotFound("Booking not found");
                }

                // Validate booking status
                if (booking.BookingStatus != "Processing")
                {
                    return ResultModel.BadRequest("Can only create results for bookings in 'Processing' status");
                }

                // Check if result already exists
                var existingResult = await _testResultRepo.GetByBookingIdAsync(createResultDto.BookingId);
                if (existingResult != null)
                {
                    return ResultModel.Conflict("Test result already exists for this booking");
                }

                // Validate user exists
                var user = await _userRepo.GetByIdAsync(issuedByUserId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                // Create test result
                var testResult = new TestResult
                {
                    BookingId = createResultDto.BookingId,
                    Notes = createResultDto.Notes,
                    IssuedBy = issuedByUserId,
                    IssuedAt = DateTime.UtcNow
                };

                var createdResult = await _testResultRepo.CreateAsync(testResult);

                // Create result details
                foreach (var detailDto in createResultDto.ResultDetails)
                {
                    var detail = new TestResultDetail
                    {
                        ResultId = createdResult.ResultId,
                        AnalyteName = detailDto.AnalyteName,
                        Value = detailDto.Value,
                        Unit = detailDto.Unit,
                        ReferenceRange = detailDto.ReferenceRange,
                        Flag = detailDto.Flag
                    };

                    await _testResultDetailRepo.CreateAsync(detail);
                }

                // Update booking status to ResultReady
                booking.BookingStatus = "ResultReady";
                booking.ResultDate = DateTime.UtcNow;
                await _testBookingRepo.UpdateAsync(booking);

                var resultDto = await MapToTestResultDto(createdResult);
                return ResultModel.Created(resultDto, "Test result created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to create test result: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetTestResultByIdAsync(int resultId)
        {
            try
            {
                var result = await _testResultRepo.GetWithDetailsAsync(resultId);
                if (result == null)
                {
                    return ResultModel.NotFound("Test result not found");
                }

                var resultDto = await MapToTestResultDto(result);
                return ResultModel.Success(resultDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get test result: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetTestResultByBookingIdAsync(int bookingId)
        {
            try
            {
                var result = await _testResultRepo.GetByBookingIdAsync(bookingId);
                if (result == null)
                {
                    return ResultModel.NotFound("Test result not found for this booking");
                }

                var resultDto = await MapToTestResultDto(result);
                return ResultModel.Success(resultDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get test result by booking: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetTestResultsByCustomerAsync(int customerId)
        {
            try
            {
                var results = await _testResultRepo.GetResultsByCustomerAsync(customerId);
                var resultDtos = new List<TestResultDto>();

                foreach (var result in results)
                {
                    resultDtos.Add(await MapToTestResultDto(result));
                }

                return ResultModel.Success(resultDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get customer test results: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateTestResultAsync(int resultId, UpdateTestResultDto updateResultDto)
        {
            try
            {
                var result = await _testResultRepo.GetWithDetailsAsync(resultId);
                if (result == null)
                {
                    return ResultModel.NotFound("Test result not found");
                }

                // Update notes if provided
                if (!string.IsNullOrEmpty(updateResultDto.Notes))
                {
                    result.Notes = updateResultDto.Notes;
                }

                // Update result details if provided
                if (updateResultDto.ResultDetails != null)
                {
                    // Remove existing details
                    foreach (var existingDetail in result.TestResultDetails.ToList())
                    {
                        await _testResultDetailRepo.RemoveAsync(existingDetail);
                    }

                    // Add new details
                    foreach (var detailDto in updateResultDto.ResultDetails)
                    {
                        var detail = new TestResultDetail
                        {
                            ResultId = result.ResultId,
                            AnalyteName = detailDto.AnalyteName,
                            Value = detailDto.Value,
                            Unit = detailDto.Unit,
                            ReferenceRange = detailDto.ReferenceRange,
                            Flag = detailDto.Flag
                        };

                        await _testResultDetailRepo.CreateAsync(detail);
                    }
                }

                await _testResultRepo.UpdateAsync(result);
                var resultDto = await MapToTestResultDto(result);
                return ResultModel.Success(resultDto, "Test result updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update test result: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetTestResultsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var results = await _testResultRepo.GetResultsByDateRangeAsync(startDate, endDate);
                var resultDtos = new List<TestResultDto>();

                foreach (var result in results)
                {
                    resultDtos.Add(await MapToTestResultDto(result));
                }

                return ResultModel.Success(resultDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get test results by date range: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteTestResultAsync(int resultId)
        {
            try
            {
                var result = await _testResultRepo.GetByIdAsync(resultId);
                if (result == null)
                {
                    return ResultModel.NotFound("Test result not found");
                }

                await _testResultRepo.RemoveAsync(result);
                return ResultModel.Success(null, "Test result deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to delete test result: {ex.Message}");
            }
        }

        private async Task<TestResultDto> MapToTestResultDto(TestResult result)
        {
            // Ensure we have all the related data
            if (result.Booking == null || result.IssuedByNavigation == null)
            {
                result = await _testResultRepo.GetWithDetailsAsync(result.ResultId) ?? result;
            }

            return new TestResultDto
            {
                ResultId = result.ResultId,
                BookingId = result.BookingId,
                CustomerName = result.Booking?.Customer != null ? 
                    $"{result.Booking.Customer.FirstName} {result.Booking.Customer.LastName}" : "Unknown",
                ServiceName = result.Booking?.Service?.ServiceName ?? "Unknown Service",
                Notes = result.Notes,
                IssuedByName = result.IssuedByNavigation != null ? 
                    $"{result.IssuedByNavigation.FirstName} {result.IssuedByNavigation.LastName}" : "Unknown",
                IssuedAt = result.IssuedAt ?? DateTime.UtcNow,
                ResultDetails = result.TestResultDetails?.Select(rd => new TestResultDetailDto
                {
                    AnalyteName = rd.AnalyteName,
                    Value = rd.Value,
                    Unit = rd.Unit,
                    ReferenceRange = rd.ReferenceRange,
                    Flag = rd.Flag
                }).ToList() ?? new List<TestResultDetailDto>()
            };
        }
    }
}