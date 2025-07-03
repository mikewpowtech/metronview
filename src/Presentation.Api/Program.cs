using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Api
{
    public class Startup(IConfiguration configuration, IWebHostEnvironment enviroment)
    {
        public IConfiguration Configuration { get; } = configuration;
        public IWebHostEnvironment Environment { get; } = enviroment;

        public void ConfigureServices(IServiceCollection services)
        {
            //service.AddSingleton<ISomeOtherService, SomeOtherService>();
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo() { Title = "App Api", Version = "v1" });
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                config.AddSecurityRequirement(
                    new OpenApiSecurityRequirement{
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type=ReferenceType.SecurityScheme,
                                    Id="Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
            });

            services.AddMvc();
            //etc
        }

        public void Configure(IApplicationBuilder app)
        {
            //if (Environment.IsDevelopment() || Environment.IsEnvironment("Test"))
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}
        }
    }

    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            var connectionStr = builder.Configuration.GetConnectionString("DefaultConnection");
            var tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>() ?? default!;
            builder.Services.AddSingleton(tokenSettings);

            //adds the database and identity setup
            builder.Services.AddInfrastructureDependencyInjection(builder.Configuration);

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromSeconds(tokenSettings.RefreshTokenExpireSeconds);
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RequireExpirationTime = true,
                        ValidIssuer = tokenSettings.Issuer,
                        ValidAudience = tokenSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.SecretKey)),
                        ClockSkew = TimeSpan.FromSeconds(0)
                    };
                });

            builder.Services.AddScoped<ApplicationDbContextInitialiser>();
            builder.Services.AddApplicationDependencyInjection(builder.Configuration);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("webAppRequests", builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(allowedOrigins ?? Array.Empty<string>())
                    .AllowCredentials();
                });
            });

            var startup = new Startup(builder.Configuration, builder.Environment);
            startup.ConfigureServices(builder.Services);

            //builder.Services.AddSwaggerGen(config =>
            //{
            //    config.SwaggerDoc("v1", new OpenApiInfo() { Title = "App Api", Version = "v1" });
            //    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //    {
            //        In = ParameterLocation.Header,
            //        Description = "Please enter token",
            //        Name = "Authorization",
            //        Type = SecuritySchemeType.Http,
            //        BearerFormat = "JWT",
            //        Scheme = "bearer"
            //    });
            //    config.AddSecurityRequirement(
            //        new OpenApiSecurityRequirement{
            //            {
            //                new OpenApiSecurityScheme
            //                {
            //                    Reference = new OpenApiReference
            //                    {
            //                        Type=ReferenceType.SecurityScheme,
            //                        Id="Bearer"
            //                    }
            //                },
            //                Array.Empty<string>()
            //            }
            //        });
            //});

            var app = builder.Build();

            startup.Configure(app);

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
            {
                using var scope = app.Services.CreateScope();
                var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
                await initialiser.InitialiseAsync();
                await initialiser.SeedAsync();
            }

            app.UseCors("webAppRequests");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            
            // Redirect root to Swagger UI
            app.MapGet("/", () => Results.Redirect("/swagger"));
            
            app.Run();
        }
    }
}