using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sentry.OpenTelemetry;
using TelemetryServer;
using TelemetryServer.Application;
using TelemetryServer.Application.Reactors;
using TelemetryServer.Infrastructure;
using TelemetryServer.Options;
using TelemetryServer.Telemetry;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd() // Does nothing unless Linux and ppid is systemd;
    .UseWindowsService() // Does nothing unless Windows and ppid is services;

    .ConfigureLogging((context, builder) =>
    {
        builder.AddConfiguration(context.Configuration);
        builder.AddSentry(options =>
        {
            options.Environment = context.HostingEnvironment.EnvironmentName.ToLowerInvariant();
            options.UseOpenTelemetry();
        });
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddOpenTelemetry()
            .WithTracing(tp =>
            {
                tp.ConfigureResource(r => r.AddService("TelemetryServer", "MetronVIEW"));
                tp.AddSource(ActivitySources.Metron2Receive.Name, ActivitySources.Metron2Transaction.Name);
                tp.AddSentry();
            });
        
        services.AddOptions();
        services.AddHostedService<TelemetryManager>();
        services.AddSingleton<SqlHelper>();
        services.AddOptions<SqlHelperOptions>().Configure(o => o.ConnectionString = configuration.GetConnectionString("Telemetry"));
        services.AddTelemetryProcessor<Metron2BulkTelemetryProcessor>();
        services.AddTelemetryProcessor<MetronAtexTelemetryProcessor>();
    })
    .Build();

await host.RunAsync();
