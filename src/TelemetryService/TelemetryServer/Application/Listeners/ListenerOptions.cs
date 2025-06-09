using System.Net;
using TelemetryServer.Application.Reactors;

namespace TelemetryServer.Application.Listeners;

public class ListenerOptions<TReactor> where TReactor : ITelemetryProcessor
{
    public string Address { get; set; }
    public IPAddress IpAddress { get; set; } = new(0L);
    public int Port { get; set; }
}