using Repository.Models;

namespace GHSMS.Services
{
    public interface IEmailService
    {
        Task SendCycleNotificationAsync(User user, string notificationType, DateTime? ovulationDate = null, DateTime? fertileWindowStart = null, DateTime? fertileWindowEnd = null);
        Task SendPillReminderAsync(User user);
        Task SendTestResultNotificationAsync(User user, string testName);
        Task SendWelcomeEmailAsync(User user);
    }
}