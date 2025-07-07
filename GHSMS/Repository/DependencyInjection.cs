using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Base;
using Repository.Models;
using Repository.Repositories;

namespace Repository
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<GenderHealthcareDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register Specific Repositories
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IRoleRepo, RoleRepo>();
            services.AddScoped<IServiceRepo, ServiceRepo>();
            services.AddScoped<ITestBookingRepo, TestBookingRepo>();
            services.AddScoped<ITestResultRepo, TestResultRepo>();
            services.AddScoped<IMenstrualCycleRepo, MenstrualCycleRepo>();

            // Register additional repositories for missing entities
            services.AddScoped<IGenericRepository<Repository.Models.Analyte>, GenericRepository<Repository.Models.Analyte>>();
            services.AddScoped<IGenericRepository<Repository.Models.TestResultDetail>, GenericRepository<Repository.Models.TestResultDetail>>();
            services.AddScoped<IGenericRepository<Repository.Models.BlogPost>, GenericRepository<Repository.Models.BlogPost>>();
            services.AddScoped<IGenericRepository<Repository.Models.Question>, GenericRepository<Repository.Models.Question>>();
            services.AddScoped<IGenericRepository<Repository.Models.Answer>, GenericRepository<Repository.Models.Answer>>();
            services.AddScoped<IGenericRepository<Repository.Models.Feedback>, GenericRepository<Repository.Models.Feedback>>();

            return services;
        }
    }
}