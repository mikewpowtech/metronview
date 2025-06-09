using System.Net;
using Microsoft.Extensions.DependencyInjection;
using TelemetryServer.Application.Listeners;
using TelemetryServer.Application.Reactors;

namespace TelemetryServer;

public static class DependencyInjection
{
    public static IServiceCollection AddTelemetryProcessor<T>(this IServiceCollection services, string configurationSectionPath = null)
        where T : ITelemetryProcessor
    {
        // comment here
        services.AddSingleton<IListener, Listener<T>>();
        services.AddOptions<ListenerOptions<T>>().BindConfiguration(configurationSectionPath ?? T.ConfigurationSection)
            .Validate(o =>
            {
                if (!string.IsNullOrWhiteSpace(o.Address))
                {
                    if (!IPAddress.TryParse(o.Address, out var ipAddress)) return false;
                    o.IpAddress = ipAddress;
                }

                if (o.Port < 0) return false;
                return true;
            })
            .ValidateOnStart();

        return services;
    }
}