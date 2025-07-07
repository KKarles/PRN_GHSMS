using Repository.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace GHSMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            _senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";
            _senderName = _configuration["EmailSettings:SenderName"] ?? "GHSMS Health Notifications";
        }

        public async Task SendCycleNotificationAsync(User user, string notificationType, DateTime? ovulationDate = null, DateTime? fertileWindowStart = null, DateTime? fertileWindowEnd = null)
        {
            try
            {
                var subject = GetCycleNotificationSubject(notificationType);
                var body = GetCycleNotificationBody(user, notificationType, ovulationDate, fertileWindowStart, fertileWindowEnd);

                await SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation($"Cycle notification sent to {user.Email} - Type: {notificationType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send cycle notification to {user.Email}");
                throw;
            }
        }

        public async Task SendPillReminderAsync(User user)
        {
            try
            {
                var subject = "💊 Daily Pill Reminder - GHSMS";
                var body = GetPillReminderBody(user);

                await SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation($"Pill reminder sent to {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send pill reminder to {user.Email}");
                throw;
            }
        }

        public async Task SendTestResultNotificationAsync(User user, string testName)
        {
            try
            {
                var subject = "🔬 Test Results Available - GHSMS";
                var body = GetTestResultNotificationBody(user, testName);

                await SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation($"Test result notification sent to {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send test result notification to {user.Email}");
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(User user)
        {
            try
            {
                var subject = "🌸 Welcome to GHSMS - Your Health Journey Starts Here";
                var body = GetWelcomeEmailBody(user);

                await SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation($"Welcome email sent to {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {user.Email}");
                throw;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

            using var message = new MailMessage();
            message.From = new MailAddress(_senderEmail, _senderName);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;

            await client.SendMailAsync(message);
        }

        private string GetCycleNotificationSubject(string notificationType)
        {
            return notificationType switch
            {
                "Ovulation" => "🌸 Ovulation Day - GHSMS Health Reminder",
                "FertileWindow" => "💖 Fertile Window Alert - GHSMS Health Reminder",
                "UpcomingOvulation" => "📅 Ovulation Approaching - GHSMS Health Reminder",
                _ => "🌸 Menstrual Cycle Update - GHSMS"
            };
        }

        private string GetCycleNotificationBody(User user, string notificationType, DateTime? ovulationDate, DateTime? fertileWindowStart, DateTime? fertileWindowEnd)
        {
            var greeting = $"Dear {user.FirstName},";
            var footer = GetEmailFooter();

            var content = notificationType switch
            {
                "Ovulation" => $@"
                    <h2 style='color: #e91e63;'>🌸 Today is Your Ovulation Day!</h2>
                    <p>Based on your cycle tracking, today ({DateTime.Today:dddd, MMMM dd, yyyy}) is your predicted ovulation day.</p>
                    <div style='background-color: #fce4ec; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                        <h3 style='color: #ad1457; margin-top: 0;'>What this means:</h3>
                        <ul style='color: #555;'>
                            <li>This is your most fertile day</li>
                            <li>If you're trying to conceive, today is optimal</li>
                            <li>If you're avoiding pregnancy, use extra protection</li>
                        </ul>
                    </div>",

                "FertileWindow" => $@"
                    <h2 style='color: #e91e63;'>💖 You're in Your Fertile Window!</h2>
                    <p>Your fertile window is from {fertileWindowStart:MMMM dd} to {fertileWindowEnd:MMMM dd, yyyy}.</p>
                    <div style='background-color: #fce4ec; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                        <h3 style='color: #ad1457; margin-top: 0;'>Important Information:</h3>
                        <ul style='color: #555;'>
                            <li>These are your most fertile days</li>
                            <li>Ovulation is expected around {ovulationDate:MMMM dd}</li>
                            <li>Plan accordingly based on your family planning goals</li>
                        </ul>
                    </div>",

                "UpcomingOvulation" => $@"
                    <h2 style='color: #e91e63;'>📅 Ovulation Approaching!</h2>
                    <p>Your ovulation is predicted for {ovulationDate:dddd, MMMM dd, yyyy} (in {(ovulationDate - DateTime.Today)?.Days} days).</p>
                    <div style='background-color: #fce4ec; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                        <h3 style='color: #ad1457; margin-top: 0;'>Get Ready:</h3>
                        <ul style='color: #555;'>
                            <li>Your fertile window is starting soon</li>
                            <li>Track any symptoms or changes</li>
                            <li>Prepare based on your family planning goals</li>
                        </ul>
                    </div>",

                _ => "<h2 style='color: #e91e63;'>🌸 Menstrual Cycle Update</h2><p>We have an update about your menstrual cycle.</p>"
            };

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>GHSMS Health Reminder</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #e91e63, #ad1457); color: white; padding: 20px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px;'>GHSMS Health Notifications</h1>
                        <p style='margin: 5px 0 0 0; opacity: 0.9;'>Your Personal Health Companion</p>
                    </div>
                    <div style='background-color: #fff; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>{greeting}</p>
                        {content}
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>💡 Health Tips:</h3>
                            <ul style='color: #666;'>
                                <li>Stay hydrated and maintain a healthy diet</li>
                                <li>Track your symptoms in the GHSMS app</li>
                                <li>Consult with our healthcare professionals if needed</li>
                            </ul>
                        </div>
                        {footer}
                    </div>
                </body>
                </html>";
        }

        private string GetPillReminderBody(User user)
        {
            var greeting = $"Dear {user.FirstName},";
            var footer = GetEmailFooter();
            var reminderTime = user.PillReminderTime?.ToString("HH:mm") ?? "your set time";

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Daily Pill Reminder</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #2196f3, #1976d2); color: white; padding: 20px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px;'>💊 Daily Pill Reminder</h1>
                        <p style='margin: 5px 0 0 0; opacity: 0.9;'>GHSMS Health Notifications</p>
                    </div>
                    <div style='background-color: #fff; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>{greeting}</p>
                        
                        <h2 style='color: #1976d2;'>💊 Time for Your Daily Pill!</h2>
                        <p>This is your friendly reminder to take your birth control pill.</p>
                        
                        <div style='background-color: #e3f2fd; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                            <h3 style='color: #1565c0; margin-top: 0;'>⏰ Reminder Details:</h3>
                            <ul style='color: #555;'>
                                <li><strong>Time:</strong> {reminderTime}</li>
                                <li><strong>Date:</strong> {DateTime.Today:dddd, MMMM dd, yyyy}</li>
                                <li><strong>Consistency is key</strong> for maximum effectiveness</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>💡 Important Reminders:</h3>
                            <ul style='color: #666;'>
                                <li>Take your pill at the same time every day</li>
                                <li>If you miss a dose, follow your healthcare provider's instructions</li>
                                <li>Contact us if you have any concerns or side effects</li>
                                <li>Keep track of your cycle in the GHSMS app</li>
                            </ul>
                        </div>
                        
                        {footer}
                    </div>
                </body>
                </html>";
        }

        private string GetTestResultNotificationBody(User user, string testName)
        {
            var greeting = $"Dear {user.FirstName},";
            var footer = GetEmailFooter();

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Test Results Available</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #4caf50, #388e3c); color: white; padding: 20px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px;'>🔬 Test Results Available</h1>
                        <p style='margin: 5px 0 0 0; opacity: 0.9;'>GHSMS Health Notifications</p>
                    </div>
                    <div style='background-color: #fff; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>{greeting}</p>
                        
                        <h2 style='color: #388e3c;'>🔬 Your Test Results Are Ready!</h2>
                        <p>Your <strong>{testName}</strong> test results are now available in your GHSMS account.</p>
                        
                        <div style='background-color: #e8f5e8; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                            <h3 style='color: #2e7d32; margin-top: 0;'>📋 Next Steps:</h3>
                            <ul style='color: #555;'>
                                <li>Log into your GHSMS account to view detailed results</li>
                                <li>Review the results with our healthcare team if needed</li>
                                <li>Follow any recommended follow-up actions</li>
                                <li>Keep your results confidential and secure</li>
                            </ul>
                        </div>
                        
                        <div style='text-align: center; margin: 25px 0;'>
                            <a href='#' style='background-color: #4caf50; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>View My Results</a>
                        </div>
                        
                        {footer}
                    </div>
                </body>
                </html>";
        }

        private string GetWelcomeEmailBody(User user)
        {
            var greeting = $"Dear {user.FirstName},";
            var footer = GetEmailFooter();

            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Welcome to GHSMS</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: linear-gradient(135deg, #e91e63, #ad1457); color: white; padding: 20px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px;'>🌸 Welcome to GHSMS!</h1>
                        <p style='margin: 5px 0 0 0; opacity: 0.9;'>Your Personal Health Journey Starts Here</p>
                    </div>
                    <div style='background-color: #fff; padding: 30px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 10px 10px;'>
                        <p style='font-size: 16px; margin-bottom: 20px;'>{greeting}</p>
                        
                        <h2 style='color: #ad1457;'>🌸 Welcome to Your Health Journey!</h2>
                        <p>Thank you for joining GHSMS - Gender Healthcare Service Management System. We're here to support your health and wellness journey with personalized care and professional services.</p>
                        
                        <div style='background-color: #fce4ec; padding: 15px; border-radius: 8px; margin: 15px 0;'>
                            <h3 style='color: #ad1457; margin-top: 0;'>🎯 What You Can Do:</h3>
                            <ul style='color: #555;'>
                                <li>Book STI testing and health screenings</li>
                                <li>Track your menstrual cycle and get predictions</li>
                                <li>Set up pill reminders and health notifications</li>
                                <li>Access your test results securely</li>
                                <li>Consult with our healthcare professionals</li>
                            </ul>
                        </div>
                        
                        <div style='background-color: #f5f5f5; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>🔐 Your Privacy Matters:</h3>
                            <p style='color: #666; margin: 0;'>All your health information is kept strictly confidential and secure. We follow the highest standards of medical privacy and data protection.</p>
                        </div>
                        
                        <div style='text-align: center; margin: 25px 0;'>
                            <a href='#' style='background-color: #e91e63; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>Explore GHSMS</a>
                        </div>
                        
                        {footer}
                    </div>
                </body>
                </html>";
        }

        private string GetEmailFooter()
        {
            return $@"
                <div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px; text-align: center; color: #666; font-size: 14px;'>
                    <p style='margin: 5px 0;'><strong>GHSMS - Gender Healthcare Service Management System</strong></p>
                    <p style='margin: 5px 0;'>Professional • Confidential • Caring</p>
                    <p style='margin: 5px 0;'>📧 Contact us: {_senderEmail}</p>
                    <p style='margin: 15px 0 5px 0; font-size: 12px; color: #999;'>
                        This email was sent to you because you have notifications enabled in your GHSMS account.<br>
                        You can manage your notification preferences in your account settings.
                    </p>
                </div>";
        }
    }
}