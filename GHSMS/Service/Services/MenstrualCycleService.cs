using Repository.Models;
using Repository.Repositories;
using Service.DTOs;
using Service.Models;

namespace Service.Services
{
    public class MenstrualCycleService : IMenstrualCycleService
    {
        private readonly IMenstrualCycleRepo _menstrualCycleRepo;
        private readonly IUserRepo _userRepo;

        public MenstrualCycleService(IMenstrualCycleRepo menstrualCycleRepo, IUserRepo userRepo)
        {
            _menstrualCycleRepo = menstrualCycleRepo;
            _userRepo = userRepo;
        }

        public async Task<ResultModel> CreateCycleAsync(int userId, CreateMenstrualCycleDto createCycleDto)
        {
            try
            {
                // Validate user exists
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    return ResultModel.NotFound("User not found");
                }

                // Check if there's an active cycle (no end date)
                var activeCycle = await _menstrualCycleRepo.GetLatestCycleByUserAsync(userId);
                if (activeCycle != null && activeCycle.EndDate == null)
                {
                    return ResultModel.BadRequest("Please end the current cycle before starting a new one");
                }

                // Validate start date
                if (createCycleDto.StartDate > DateTime.UtcNow)
                {
                    return ResultModel.BadRequest("Start date cannot be in the future");
                }

                var cycle = new MenstrualCycle
                {
                    UserId = userId,
                    StartDate = DateOnly.FromDateTime(createCycleDto.StartDate),
                    EndDate = createCycleDto.EndDate.HasValue ? DateOnly.FromDateTime(createCycleDto.EndDate.Value) : null,
                    ExpectedCycleLength = createCycleDto.ExpectedCycleLength
                };

                var createdCycle = await _menstrualCycleRepo.CreateAsync(cycle);
                var cycleDto = MapToMenstrualCycleDto(createdCycle);
                return ResultModel.Created(cycleDto, "Menstrual cycle created successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to create cycle: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCyclesByUserAsync(int userId)
        {
            try
            {
                var cycles = await _menstrualCycleRepo.GetCyclesByUserAsync(userId);
                var cycleDtos = cycles.Select(MapToMenstrualCycleDto).ToList();
                return ResultModel.Success(cycleDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get cycles: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCycleByIdAsync(int cycleId)
        {
            try
            {
                var cycle = await _menstrualCycleRepo.GetByIdAsync(cycleId);
                if (cycle == null)
                {
                    return ResultModel.NotFound("Cycle not found");
                }

                var cycleDto = MapToMenstrualCycleDto(cycle);
                return ResultModel.Success(cycleDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get cycle: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateCycleAsync(int cycleId, int userId, UpdateMenstrualCycleDto updateCycleDto)
        {
            try
            {
                var cycle = await _menstrualCycleRepo.GetByIdAsync(cycleId);
                if (cycle == null)
                {
                    return ResultModel.NotFound("Cycle not found");
                }

                if (cycle.UserId != userId)
                {
                    return ResultModel.Forbidden("You can only update your own cycles");
                }

                // Update fields if provided
                if (updateCycleDto.EndDate.HasValue)
                {
                    if (updateCycleDto.EndDate.Value <= cycle.StartDate.ToDateTime(TimeOnly.MinValue))
                    {
                        return ResultModel.BadRequest("End date must be after start date");
                    }
                    cycle.EndDate = DateOnly.FromDateTime(updateCycleDto.EndDate.Value);
                }

                if (updateCycleDto.ExpectedCycleLength.HasValue)
                {
                    cycle.ExpectedCycleLength = updateCycleDto.ExpectedCycleLength.Value;
                }

                await _menstrualCycleRepo.UpdateAsync(cycle);
                var cycleDto = MapToMenstrualCycleDto(cycle);
                return ResultModel.Success(cycleDto, "Cycle updated successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to update cycle: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteCycleAsync(int cycleId, int userId)
        {
            try
            {
                var cycle = await _menstrualCycleRepo.GetByIdAsync(cycleId);
                if (cycle == null)
                {
                    return ResultModel.NotFound("Cycle not found");
                }

                if (cycle.UserId != userId)
                {
                    return ResultModel.Forbidden("You can only delete your own cycles");
                }

                await _menstrualCycleRepo.RemoveAsync(cycle);
                return ResultModel.Success(null, "Cycle deleted successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to delete cycle: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetLatestCycleAsync(int userId)
        {
            try
            {
                var cycle = await _menstrualCycleRepo.GetLatestCycleByUserAsync(userId);
                if (cycle == null)
                {
                    return ResultModel.NotFound("No cycles found for user");
                }

                var cycleDto = MapToMenstrualCycleDto(cycle);
                return ResultModel.Success(cycleDto);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get latest cycle: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCyclePredictionsAsync(int userId)
        {
            try
            {
                var cycles = await _menstrualCycleRepo.GetCyclesByUserAsync(userId);
                var completedCycles = cycles.Where(c => c.EndDate.HasValue).ToList();

                if (completedCycles.Count < 2)
                {
                    return ResultModel.Success(new CyclePredictionDto
                    {
                        HasSufficientData = false,
                        Message = "Need at least 2 completed cycles for predictions"
                    });
                }

                // Calculate average cycle length
                var cycleLengths = completedCycles.Select(c => 
                    (c.EndDate!.Value.ToDateTime(TimeOnly.MinValue) - c.StartDate.ToDateTime(TimeOnly.MinValue)).Days).ToList();
                
                var averageCycleLength = cycleLengths.Average();

                // Get the latest cycle
                var latestCycle = cycles.OrderByDescending(c => c.StartDate).First();
                var lastPeriodStart = latestCycle.StartDate.ToDateTime(TimeOnly.MinValue);

                // Calculate predictions
                var nextPeriodDate = lastPeriodStart.AddDays(averageCycleLength);
                var ovulationDate = nextPeriodDate.AddDays(-14); // Ovulation typically 14 days before next period
                var fertileWindowStart = ovulationDate.AddDays(-5); // Fertile window starts 5 days before ovulation
                var fertileWindowEnd = ovulationDate.AddDays(1); // Fertile window ends 1 day after ovulation

                var prediction = new CyclePredictionDto
                {
                    NextPeriodDate = nextPeriodDate,
                    OvulationDate = ovulationDate,
                    FertileWindowStart = fertileWindowStart,
                    FertileWindowEnd = fertileWindowEnd,
                    AverageCycleLength = averageCycleLength,
                    HasSufficientData = true,
                    Message = $"Predictions based on {completedCycles.Count} completed cycles"
                };

                return ResultModel.Success(prediction);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get cycle predictions: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetActiveCyclesAsync()
        {
            try
            {
                var cycles = await _menstrualCycleRepo.GetActiveCyclesAsync();
                var cycleDtos = cycles.Select(MapToMenstrualCycleDto).ToList();
                return ResultModel.Success(cycleDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get active cycles: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetCyclesByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var cycles = await _menstrualCycleRepo.GetCyclesByDateRangeAsync(userId, startDate, endDate);
                var cycleDtos = cycles.Select(MapToMenstrualCycleDto).ToList();
                return ResultModel.Success(cycleDtos);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get cycles by date range: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetUsersNeedingNotificationsAsync()
        {
            try
            {
                // Get users who want cycle notifications
                var users = await _userRepo.GetUsersWithCycleNotificationsAsync();
                var usersNeedingNotifications = new List<object>();

                foreach (var user in users)
                {
                    // Get user's latest active cycle
                    var latestCycle = await _menstrualCycleRepo.GetLatestCycleByUserAsync(user.UserId);
                    if (latestCycle != null && !latestCycle.EndDate.HasValue)
                    {
                        var cycleDto = MapToMenstrualCycleDto(latestCycle);
                        
                        // Check if user needs notifications (approaching ovulation, fertile window, etc.)
                        var today = DateTime.Today;
                        var startDate = latestCycle.StartDate.ToDateTime(TimeOnly.MinValue);
                        
                        // Predict ovulation (typically 14 days before next period)
                        var predictedOvulation = startDate.AddDays(latestCycle.ExpectedCycleLength - 14);
                        var daysToOvulation = (predictedOvulation - today).Days;
                        
                        // Fertile window (5 days before ovulation to 1 day after)
                        var fertileWindowStart = predictedOvulation.AddDays(-5);
                        var fertileWindowEnd = predictedOvulation.AddDays(1);
                        
                        var isInFertileWindow = today >= fertileWindowStart && today <= fertileWindowEnd;
                        var isOvulationDay = today.Date == predictedOvulation.Date;
                        
                        if (isOvulationDay || isInFertileWindow || daysToOvulation <= 2)
                        {
                            usersNeedingNotifications.Add(new
                            {
                                User = new
                                {
                                    user.UserId,
                                    user.FirstName,
                                    user.LastName,
                                    user.Email
                                },
                                Cycle = cycleDto,
                                NotificationType = isOvulationDay ? "Ovulation" : 
                                                 isInFertileWindow ? "FertileWindow" : "UpcomingOvulation",
                                DaysToOvulation = daysToOvulation
                            });
                        }
                    }
                }

                return ResultModel.Success(usersNeedingNotifications);
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to get users needing notifications: {ex.Message}");
            }
        }

        public async Task<ResultModel> EndCurrentCycleAsync(int userId, DateTime endDate)
        {
            try
            {
                var activeCycle = await _menstrualCycleRepo.GetLatestCycleByUserAsync(userId);
                if (activeCycle == null || activeCycle.EndDate.HasValue)
                {
                    return ResultModel.NotFound("No active cycle found");
                }

                if (endDate <= activeCycle.StartDate.ToDateTime(TimeOnly.MinValue))
                {
                    return ResultModel.BadRequest("End date must be after start date");
                }

                activeCycle.EndDate = DateOnly.FromDateTime(endDate);
                await _menstrualCycleRepo.UpdateAsync(activeCycle);

                var cycleDto = MapToMenstrualCycleDto(activeCycle);
                return ResultModel.Success(cycleDto, "Cycle ended successfully");
            }
            catch (Exception ex)
            {
                return ResultModel.InternalServerError($"Failed to end cycle: {ex.Message}");
            }
        }

        private MenstrualCycleDto MapToMenstrualCycleDto(MenstrualCycle cycle)
        {
            var startDate = cycle.StartDate.ToDateTime(TimeOnly.MinValue);
            var endDate = cycle.EndDate?.ToDateTime(TimeOnly.MinValue);

            // Calculate actual cycle length if cycle is completed
            int? actualCycleLength = null;
            if (endDate.HasValue)
            {
                actualCycleLength = (endDate.Value - startDate).Days;
            }

            // Calculate predictions based on expected cycle length
            var predictedNextPeriod = startDate.AddDays(cycle.ExpectedCycleLength);
            var predictedOvulation = predictedNextPeriod.AddDays(-14);
            var fertileWindowStart = predictedOvulation.AddDays(-5);
            var fertileWindowEnd = predictedOvulation.AddDays(1);

            return new MenstrualCycleDto
            {
                CycleId = cycle.CycleId,
                UserId = cycle.UserId,
                StartDate = startDate,
                EndDate = endDate,
                ExpectedCycleLength = cycle.ExpectedCycleLength,
                ActualCycleLength = actualCycleLength,
                PredictedNextPeriod = endDate.HasValue ? null : predictedNextPeriod,
                PredictedOvulation = endDate.HasValue ? null : predictedOvulation,
                FertileWindowStart = endDate.HasValue ? null : fertileWindowStart,
                FertileWindowEnd = endDate.HasValue ? null : fertileWindowEnd
            };
        }
    }
}