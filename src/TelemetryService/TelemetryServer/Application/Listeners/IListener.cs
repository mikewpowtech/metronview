using System.Threading;
using System.Threading.Tasks;

namespace TelemetryServer.Application.Listeners;

public interface IListener
{
    Task StartListeningAsync(CancellationToken stoppingToken);
}