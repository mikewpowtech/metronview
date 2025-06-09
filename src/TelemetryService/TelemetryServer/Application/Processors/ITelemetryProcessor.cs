using System.Threading;
using System.Threading.Tasks;

namespace TelemetryServer.Application.Reactors
{
    public interface ITelemetryProcessor
    {
        static abstract string ConfigurationSection { get; }
        Task ProcessTelemetryAsync(CancellationToken cancellationToken);
    }

}
