using Application.Alarms;
using Application.ConfigurationUploads;
using Application.CustomFields;
using Application.Messaging;
using Application.Options;
using Application.Readings;
using Application.Recipients;
using Application.Sensors;
using Application.Triggers;
using Application.Units;
using Infrastructure;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Presentation.AlarmServer.Data.SpiderScope;
using Presentation.AlarmServer.Options;
using Presentation.AlarmServer.ServiceWorkers;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSystemd()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        // Register WorkerOptions
        services.Configure<WorkerOptions>(configuration.GetSection("WorkerOptions"));
        // Register SpiderScopeApiOptions
        services.Configure<SpiderScopeApiOptions>(configuration.GetSection("SpiderScopeApiOptions"));
        // Add services to the container.
        services.AddHttpClient<ISpiderScopeApi, SpiderScopeApi>();

        // Register AlarmDbContext with connection string from config
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TelemetryDatabaseConnectionString")));
        // Register Mapster
        services.AddMapster();
        // Register your mapping configuration
        MapsterMappingConfig.RegisterMappings();

        services.AddOptions<SmtpOptions>()
            .BindConfiguration("Smtp")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        //services.AddSingleton<ISqlConnectionProvider, SqlConnectionProvider>();
        //services.AddSingleton<ITelemetryDatabase,TelemetryDatabase>();
        var sp = services.BuildServiceProvider();
        services.AddTransient<IRecipientService, RecipientService>();
        services.AddTransient<IRecipientRepository, RecipientRepository>();
        services.AddTransient<IUnitRepository, UnitRepository>();
        services.AddTransient<ISensorService, SensorService>();
        services.AddTransient<IReadingService, ReadingService>();
        services.AddTransient<IReadingRepository, ReadingRepository>();
        services.AddTransient<ISensorRepository, SensorRepository>();
        services.AddTransient<ICustomFieldService, CustomFieldService>();
        services.AddTransient<IUnitService, UnitService>();
        services.AddTransient<IUnitRepository, UnitRepository>();
        services.AddTransient<IConfigurationUploadService, ConfigurationUploadService>();
        services.AddTransient<ICustomFieldRepository, CustomFieldRepository>();
        services.AddTransient<IConfigurationUploadRepository, ConfigurationUploadRepository>();
        services.AddTransient<IMessengerService, MessengerService>();
        services.AddTransient<IAlarmRepository, AlarmRepository>();
        services.AddTransient<ITriggerRepository, TriggerRepository>();
        services.AddTransient<IAlarmService, AlarmService>();
        services.AddTransient<ITriggerService, TriggerService>();

        services.AddHostedService<AlarmWorkerService>();
        services.AddHostedService<NotReportedAlarmWorkerService>();
        services.AddHostedService<HenkelWorkerService>();
    })
    .Build();

//ValueCalculator.Logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ValueCalculator"); //wtf?
Console.WriteLine($"Final Environment: {host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName}");
Console.WriteLine($"DOTNET_ENVIRONMENT: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"BaseDir: {AppContext.BaseDirectory}");
Console.WriteLine($"Dev config exists: {File.Exists(Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json"))}");
Console.WriteLine("Use CTRL+C to stop service");
await host.RunAsync();

