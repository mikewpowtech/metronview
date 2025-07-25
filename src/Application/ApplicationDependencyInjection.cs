using Application.Alarms;
using Application.Companies;
using Application.CommunicationModes;
using Application.ConfigurationUploads;
using Application.Dashboard;
using Application.Identity;
using Application.Readings;
using Application.Sensors;
using Application.Triggers;
using Application.TriggerTypes;
using Application.Units;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Messaging;

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
            services.AddScoped<IUnitModelService, UnitModelService>();
            services.AddScoped<IReadingService, ReadingService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IConfigurationUploadService, ConfigurationUploadService>();
            services.AddScoped<IMessengerService, MessengerService>();
            services.AddScoped<IAlarmService, AlarmService>();
            services.AddScoped<ITriggerTypeService, TriggerTypeService>();
            services.AddScoped<ICommunicationModeService, CommunicationModeService>();
            services.AddScoped<ITriggerService, TriggerService>();

            // Register Mapster for mapping
            services.AddMapster();

            // Register other application-level services here as needed
            // services.AddScoped<IOtherService, OtherService>();

            return services;
        }
    }
}