using Application.Alarms;
using Application.Messaging;
using Application.Options;
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


Console.WriteLine($"ENV: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"BaseDir: {AppContext.BaseDirectory}");
Console.WriteLine($"Dev config exists: {File.Exists(Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json"))}");

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
        services.AddHttpClient<ISpiderScopeApi,SpiderScopeApi>();

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
        var sp=services.BuildServiceProvider();
        services.AddTransient<IRecipientService, RecipientService>();
        services.AddTransient<IRecipientRepository, RecipientRepository>();
        services.AddTransient<IUnitRepository, UnitRepository>();
        services.AddTransient<ISensorService, SensorService>();
        services.AddTransient<ISensorRepository, SensorRepository>();
        services.AddTransient<IMessengerService,MessengerService>();
        services.AddTransient<IAlarmRepository,AlarmRepository>();
        services.AddTransient<ITriggerRepository,TriggerRepository>();
        services.AddTransient<IAlarmService, AlarmService>();
        services.AddTransient<ITriggerService, TriggerService>();
        services.AddTransient<NotReportedServiceWorker>();
        services.AddTransient<AlarmServiceWorker>();
        services.AddTransient<HenkelServiceWorker>();
        //services.AddHostedService<HostedService<HenkelServiceWorker>>();
        //services.AddHostedService<HostedService<AlarmServiceWorker>>();
        services.AddHostedService<HostedService<NotReportedServiceWorker>>();
    })
    .Build();

//ValueCalculator.Logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ValueCalculator"); //wtf?
Console.WriteLine($"ENV: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"BaseDir: {AppContext.BaseDirectory}");
Console.WriteLine($"Dev config exists: {File.Exists(Path.Combine(AppContext.BaseDirectory, "appsettings.Development.json"))}");
Console.WriteLine("Use CTRL+C to stop service");
await host.RunAsync();

