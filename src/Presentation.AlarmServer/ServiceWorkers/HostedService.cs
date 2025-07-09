using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Presentation.AlarmServer.ServiceWorkers;

public class HostedService<T> : BackgroundService where  T:IHostedServiceWorker
{
    private T Poller { get; }
    private string className { get; init; }

    public HostedService(T poller)
    {
        Poller = poller;
        className = poller.GetType().Name;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() => Poller.Run(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }
}