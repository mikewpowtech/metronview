using Application.Companies;
using Application.Identity;
using Application.Units;
using Application.Sensors;
using Infrastructure.Identity;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Readings;

namespace Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructureDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            var connectionStr = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionStr, x => x.MigrationsAssembly("Infrastructure")));

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

            // Register DbContext Initializer
            services.AddScoped<ApplicationDbContextInitialiser>();

            return services;
        }
    }
}