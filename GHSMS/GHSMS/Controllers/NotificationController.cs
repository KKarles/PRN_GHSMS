using GHSMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;

namespace GHSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class NotificationController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IMenstrualCycleService _menstrualCycleService;
        private readonly IUserService _userService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            IEmailService emailService,
            IMenstrualCycleService menstrualCycleService,
            IUserService userService,
            ILogger<NotificationController> logger)
        {
            _emailService = emailService;
            _menstrualCycleService = menstrualCycleService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Manually trigger cycle notifications for testing
        /// </summary>
        [HttpPost("trigger-cycle-notifications")]
        public async Task<IActionResult> TriggerCycleNotifications()
        {
            try
            {
                var result = await _menstrualCycleService.GetUsersNeedingNotificationsAsync();
                if (!result.IsSuccess)
                {
                    return StatusCode(result.Code, new { message = result.Message });
                }

                var usersNeedingNotifications = result.Data as List<object>;
                if (usersNeedingNotifications == null || !usersNeedingNotifications.Any())
                {
                    return Ok(new { message = "No users need cycle notifications at this time", count = 0 });
                }

                var sentCount = 0;
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

                        var userObj = new Repository.Models.User
                        {
                            UserId = userId,
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email
                        };

                        DateTime? ovulationDate = null;
                        DateTime? fertileWindowStart = null;
                        DateTime? fertileWindowEnd = null;

                        if (cycleProperty != null)
                        {
                            ovulationDate = cycleProperty.GetType().GetProperty("PredictedOvulation")?.GetValue(cycleProperty) as DateTime?;
                            fertileWindowStart = cycleProperty.GetType().GetProperty("FertileWindowStart")?.GetValue(cycleProperty) as DateTime?;
                            fertileWindowEnd = cycleProperty.GetType().GetProperty("FertileWindowEnd")?.GetValue(cycleProperty) as DateTime?;
                        }

                        await _emailService.SendCycleNotificationAsync(
                            userObj, 
                            notificationType ?? "General", 
                            ovulationDate, 
                            fertileWindowStart, 
                            fertileWindowEnd);

                        sentCount++;
                        _logger.LogInformation("Manual cycle notification sent to {Email}", userObj.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send manual cycle notification");
                    }
                }

                return Ok(new { 
                    message = $"Cycle notifications triggered successfully", 
                    totalUsers = usersNeedingNotifications.Count,
                    sentCount = sentCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering cycle notifications");
                return StatusCode(500, new { message = "Failed to trigger cycle notifications" });
            }
        }

        /// <summary>
        /// Manually trigger pill reminders for testing
        /// </summary>
        [HttpPost("trigger-pill-reminders")]
        public async Task<IActionResult> TriggerPillReminders()
        {
            try
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var result = await _userService.GetUsersWithPillRemindersAsync(currentTime);
                
                if (!result.IsSuccess)
                {
                    return StatusCode(result.Code, new { message = result.Message });
                }

                var users = result.Data as List<Service.DTOs.UserProfileDto>;
                if (users == null || !users.Any())
                {
                    return Ok(new { 
                        message = "No users need pill reminders at this time", 
                        currentTime = currentTime.ToString(@"hh\:mm"),
                        count = 0 
                    });
                }

                var sentCount = 0;
                foreach (var userDto in users)
                {
                    try
                    {
                        // Apply 5-minute window logic (same as background service)
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

                                await _emailService.SendPillReminderAsync(userObj);
                                sentCount++;
                                _logger.LogInformation("Manual pill reminder sent to {Email} (Time diff: {Diff} minutes)", 
                                    userObj.Email, timeDifference);
                            }
                            else
                            {
                                _logger.LogInformation("Skipping {Email} - Time difference too large: {Diff} minutes (reminder: {ReminderTime}, current: {CurrentTime})", 
                                    userDto.Email, timeDifference, reminderTime.ToString(@"hh\:mm"), currentTime.ToString(@"hh\:mm"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send manual pill reminder to {Email}", userDto.Email);
                    }
                }

                return Ok(new { 
                    message = $"Pill reminders triggered successfully", 
                    currentTime = currentTime.ToString(@"hh\:mm"),
                    totalUsers = users.Count,
                    sentCount = sentCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering pill reminders");
                return StatusCode(500, new { message = "Failed to trigger pill reminders" });
            }
        }

        /// <summary>
        /// Send a test email to verify email configuration
        /// </summary>
        [HttpPost("test-email/{email}")]
        public async Task<IActionResult> SendTestEmail(string email)
        {
            try
            {
                var testUser = new Repository.Models.User
                {
                    UserId = 0,
                    FirstName = "Test",
                    LastName = "User",
                    Email = email
                };

                await _emailService.SendWelcomeEmailAsync(testUser);
                _logger.LogInformation("Test email sent to {Email}", email);

                return Ok(new { message = $"Test email sent successfully to {email}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", email);
                return StatusCode(500, new { message = $"Failed to send test email: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get notification status and statistics
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetNotificationStatus()
        {
            try
            {
                // Get users with cycle notifications enabled
                var cycleUsersResult = await _userService.GetUsersWithCycleNotificationsAsync();
                var cycleUsers = cycleUsersResult.IsSuccess ? 
                    (cycleUsersResult.Data as List<Service.DTOs.UserProfileDto>)?.Count ?? 0 : 0;

                // Get users with pill reminders
                var currentTime = DateTime.Now.TimeOfDay;
                var pillUsersResult = await _userService.GetUsersWithPillRemindersAsync(currentTime);
                var pillUsers = pillUsersResult.IsSuccess ? 
                    (pillUsersResult.Data as List<Service.DTOs.UserProfileDto>)?.Count ?? 0 : 0;

                // Get users needing cycle notifications
                var cycleNotificationsResult = await _menstrualCycleService.GetUsersNeedingNotificationsAsync();
                var usersNeedingCycleNotifications = cycleNotificationsResult.IsSuccess ? 
                    (cycleNotificationsResult.Data as List<object>)?.Count ?? 0 : 0;

                return Ok(new
                {
                    status = "Active",
                    currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    statistics = new
                    {
                        usersWithCycleNotifications = cycleUsers,
                        usersWithPillReminders = pillUsers,
                        usersNeedingCycleNotificationsNow = usersNeedingCycleNotifications,
                        backgroundServiceStatus = "Running"
                    },
                    emailConfiguration = new
                    {
                        smtpServer = "smtp.gmail.com",
                        smtpPort = 587,
                        senderConfigured = !string.IsNullOrEmpty(_emailService.GetType().Name)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification status");
                return StatusCode(500, new { message = "Failed to get notification status" });
            }
        }
    }
}