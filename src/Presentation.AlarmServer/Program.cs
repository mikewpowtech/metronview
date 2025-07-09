using Presentation.AlarmServer.Data.TelemetrySQL;
using Presentation.AlarmServer.Data.SpiderScope;
using Presentation.AlarmServer.Options;
using Presentation.AlarmServer.Email;
using Presentation.AlarmServer.Alarms;
using Presentation.AlarmServer.ServiceWorkers;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Infrastructure;
using Application.Alarms;
using Infrastructure.Repositories;
using Mapster;
using Infrastructure.Mapping;


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

        services.AddSingleton<ISqlConnectionProvider, SqlConnectionProvider>();
        services.AddSingleton<ITelemetryDatabase,TelemetryDatabase>();
        var sp=services.BuildServiceProvider();
        services.AddTransient<IMessengerService,MessengerService>();
        services.AddTransient<IAlarmServerService,AlarmServerService>();
        services.AddTransient<ISensorServerService,SensorServerService>();
        services.AddTransient<IAlarmRepository,AlarmRepository>();
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

