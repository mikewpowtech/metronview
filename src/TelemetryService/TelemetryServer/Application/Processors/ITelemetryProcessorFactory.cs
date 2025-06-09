using System.Net.Sockets;

namespace TelemetryServer.Application.Reactors;

/// <summary>
/// The interface that allows manufacture of arbitrary Reactors to handle communication with devices.
/// </summary>
public interface ITelemetryProcessorFactory
{
    ITelemetryProcessor CreateTelemetryProcessor(TcpClient tcpClient);
}
