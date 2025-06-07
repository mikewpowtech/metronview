using Application.Companies;
using Application.Identity;
using Application.Sensors;
using Application.Units;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplicationDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            // Register application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IUnitService, UnitService>();
            services.AddScoped<ISensorService, SensorService>();

            // Register Mapster for mapping
            services.AddMapster();

            // Register other application-level services here as needed
            // services.AddScoped<IOtherService, OtherService>();

            return services;
        }
    }
}