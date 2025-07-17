using Application.Alarms;
using Application.Companies;
using Application.CommunicationModes;
using Application.ConfigurationUploads;
using Application.Dashboard;
using Application.Identity;
using Application.Readings;
using Application.Recipients;
using Application.RecipientSets;
using Application.Sensors;
using Application.Triggers;
using Application.TriggerTypes;
using Application.Units;
using Infrastructure.Identity;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.CustomFields;

namespace Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            var connectionStr = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionStr, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly("Infrastructure");
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(60); // Set command timeout to 60 seconds
                }));

            // Register Identity
            services.AddIdentity<ApplicationUserDb, IdentityRole>()
                .AddSignInManager()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddTokenProvider<DataProtectorTokenProvider<ApplicationUserDb>>("REFRESHTOKENPROVIDER");

            // Register Mapster
            services.AddMapster();

            // Register your mapping configuration
            MapsterMappingConfig.RegisterMappings();

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
            services.AddScoped<ISensorRepository, SensorRepository>();
            services.AddScoped<IUnitModelRepository, UnitModelRepository>();
            services.AddScoped<IReadingRepository, ReadingRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IConfigurationUploadRepository, ConfigurationUploadRepository>();
            services.AddScoped<IAlarmRepository, AlarmRepository>();
            services.AddScoped<ITriggerTypeRepository, TriggerTypeRepository>();
            services.AddScoped<ICommunicationModeRepository, CommunicationModeRepository>();
            services.AddScoped<ITriggerRepository, TriggerRepository>();
            services.AddScoped<IRecipientRepository, RecipientRepository>();
            services.AddScoped<IRecipientService, RecipientService>();
            services.AddScoped<IRecipientSetRepository, RecipientSetRepository>();
            services.AddScoped<IRecipientSetService, RecipientSetService>();
            services.AddScoped<ICustomFieldRepository, CustomFieldRepository>();
            services.AddScoped<ICustomFieldService, CustomFieldService>();

            // Register DbContext Initializer
            services.AddScoped<ApplicationDbContextInitialiser>();

            return services;
        }
    }
}