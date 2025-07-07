using GHSMS.Services;
using Service.Services;

namespace GHSMS.BackgroundServices
{
    public class HealthNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthNotificationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(280); // Check every hour

        public HealthNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<HealthNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Health Notification Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessHealthNotifications();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Health Notification Background Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Health Notification Background Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retry
                }
            }
        }

        private async Task ProcessHealthNotifications()
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var menstrualCycleService = scope.ServiceProvider.GetRequiredService<IMenstrualCycleService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            _logger.LogInformation("Processing health notifications at {Time}", DateTime.Now);

            // Process menstrual cycle notifications
            await ProcessCycleNotifications(emailService, menstrualCycleService);

            // Process pill reminders
            await ProcessPillReminders(emailService, userService);
        }

        private async Task ProcessCycleNotifications(IEmailService emailService, IMenstrualCycleService menstrualCycleService)
        {
            try
            {
                var result = await menstrualCycleService.GetUsersNeedingNotificationsAsync();
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to get users needing cycle notifications: {Message}", result.Message);
                    return;
                }

                var usersNeedingNotifications = result.Data as List<object>;
                if (usersNeedingNotifications == null || !usersNeedingNotifications.Any())
                {
                    _logger.LogInformation("No users need cycle notifications at this time");
                    return;
                }

                foreach (var notification in usersNeedingNotifications)
                {
                    try
                    {
                        // Use reflection to access anonymous object properties
                        var notificationType = notification.GetType().GetProperty("NotificationType")?.GetValue(notification)?.ToString();
                        var userProperty = notification.GetType().GetProperty("User")?.GetValue(notification);
                        var cycleProperty = notification.GetType().GetProperty("Cycle")?.GetValue(notification);

                        if (userProperty == null) continue;

                        // Extract user properties
                        var userId = (int)(userProperty.GetType().GetProperty("UserId")?.GetValue(userProperty) ?? 0);
                        var firstName = userProperty.GetType().GetProperty("FirstName")?.GetValue(userProperty)?.ToString() ?? "";
                        var lastName = userProperty.GetType().GetProperty("LastName")?.GetValue(userProperty)?.ToString() ?? "";
                        var email = userProperty.GetType().GetProperty("Email")?.GetValue(userProperty)?.ToString() ?? "";

                        // Create a User object for the email service
                        var userObj = new Repository.Models.User
                        {
                            UserId = userId,
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email
                        };

                        // Extract dates from cycle if available
                        DateTime? ovulationDate = null;
                        DateTime? fertileWindowStart = null;
                        DateTime? fertileWindowEnd = null;

                        if (cycleProperty != null)
                        {
                            ovulationDate = cycleProperty.GetType().GetProperty("PredictedOvulation")?.GetValue(cycleProperty) as DateTime?;
                            fertileWindowStart = cycleProperty.GetType().GetProperty("FertileWindowStart")?.GetValue(cycleProperty) as DateTime?;
                            fertileWindowEnd = cycleProperty.GetType().GetProperty("FertileWindowEnd")?.GetValue(cycleProperty) as DateTime?;
                        }

                        await emailService.SendCycleNotificationAsync(
                            userObj, 
                            notificationType ?? "General", 
                            ovulationDate, 
                            fertileWindowStart, 
                            fertileWindowEnd);

                       // _logger.LogInformation("Cycle notification sent to {Email} - Type: {Type}", 
                          //  userObj.Email, notificationType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send cycle notification to user");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cycle notifications");
            }
        }

        private async Task ProcessPillReminders(IEmailService emailService, IUserService userService)
        {
            try
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var result = await userService.GetUsersWithPillRemindersAsync(currentTime);
                
                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to get users needing pill reminders: {Message}", result.Message);
                    return;
                }

                var users = result.Data as List<Service.DTOs.UserProfileDto>;
                if (users == null || !users.Any())
                {
                    _logger.LogInformation("No users need pill reminders at this time ({Time})", currentTime.ToString(@"hh\:mm"));
                    return;
                }

                foreach (var userDto in users)
                {
                    try
                    {
                        // Check if we should send reminder (within 5 minutes of set time)
                        if (userDto.PillReminderTime.HasValue)
                        {
                            var reminderTime = userDto.PillReminderTime.Value;
                            var timeDifference = Math.Abs((currentTime - reminderTime).TotalMinutes);
                            
                            if (timeDifference <= 5) // Within 5 minutes of reminder time
                            {
                                var userObj = new Repository.Models.User
                                {
                                    UserId = userDto.UserId,
                                    FirstName = userDto.FirstName,
                                    LastName = userDto.LastName,
                                    Email = userDto.Email,
                                    PillReminderTime = TimeOnly.FromTimeSpan(reminderTime)
                                };

                                await emailService.SendPillReminderAsync(userObj);
                                _logger.LogInformation("Pill reminder sent to {Email} at {Time}", 
                                    userObj.Email, currentTime.ToString(@"hh\:mm"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send pill reminder to {Email}", userDto.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pill reminders");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Health Notification Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}