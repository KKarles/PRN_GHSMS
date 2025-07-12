using Microsoft.Extensions.DependencyInjection;
using Service.Services;

namespace Service
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServiceLayer(this IServiceCollection services)
        {
            // Register Service Layer Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
            services.AddScoped<ITestBookingService, TestBookingService>();
            services.AddScoped<ITestResultService, TestResultService>();
            services.AddScoped<IMenstrualCycleService, MenstrualCycleService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ICustomerProfileService, CustomerProfileService>();
            services.AddScoped<IStaffQualificationService, StaffQualificationService>();
            services.AddScoped<IStaffDashboardService, StaffDashboardService>();
            services.AddScoped<IUserLookupService, UserLookupService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IAppointmentService, AppointmentService>();

            return services;
        }
    }
}