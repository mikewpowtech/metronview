using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Powelectrics.Telemetry.FileProcessor;
using System.Runtime.InteropServices;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddHostedService<Manager>();
            });

        // To run this without it trying to daemonise on you, add a first argument of "--interactive"
        if (args.Length > 0 && !"--interactive".Equals(args[0]))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                hostBuilder.UseWindowsService();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                hostBuilder.UseSystemd();
        }

        IHost host = hostBuilder.Build();

        host.RunAsync().Wait();
    }
}
