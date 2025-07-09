using System.Threading;

namespace Presentation.AlarmServer.ServiceWorkers;

public interface IHostedServiceWorker
{
    public void Run(CancellationToken stoppingToken);
}