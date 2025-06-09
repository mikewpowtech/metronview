using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TelemetryServer.Application.Reactors;

namespace TelemetryServer.Application.Listeners;

/// <summary>
/// Reads the service configuration and binds appropriate Reactors to the defined address(es) and port(s).
/// This then handles incoming connections on the port(s), handing them off to Reactors as required.
/// </summary>
/// <remarks>Listener is responsible for stopping itself (but not any active Reactors) when the service stops.
/// 
/// This class must be thread-safe.</remarks>
public class Listener : IListener
{
    /// <summary>
    /// The number of incoming connections that may be queued pending an accept.
    /// Connections beyond this will not be acknowledged by the network stack.
    /// </summary>
    private const int BACKLOG = 10000;

    private readonly TcpListener tcpListener;
    private readonly ITelemetryProcessorFactory telemetryProcessorFactory;
    private readonly ILogger _logger;
    private Task _listenerTask;


    /// <summary>
    /// Set up this Listener so that when a new connection comes in, it will create telemetryProcessors from telemetryProcessorFactory.
    /// </summary>
    /// <param name="telemetryProcessorFactory"></param>
    /// <param name="ipAddress"> </param>
    /// <param name="port"> </param>
    /// <param name="logger"></param>
    public Listener(ITelemetryProcessorFactory telemetryProcessorFactory, IPAddress ipAddress, int port, ILogger logger)
    {
        this.telemetryProcessorFactory = telemetryProcessorFactory;
        _logger = logger;

        IPEndPoint localEndPoint = new(ipAddress, port);
        tcpListener = new TcpListener(localEndPoint);
    }

    public Task StartListeningAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Start listening at {@EndPoint} ", tcpListener.LocalEndpoint);
        tcpListener.Start(BACKLOG);

        _listenerTask = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await tcpListener.AcceptTcpClientAsync(stoppingToken);

                    var telemetryProcessor = telemetryProcessorFactory.CreateTelemetryProcessor(client);

                    _ = TelemetryManager.NoteNewTelemetryProcessorTask(telemetryProcessor.ProcessTelemetryAsync(stoppingToken));
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Shutting down");
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Exception while accepting");
                }
            }

            // If we get here, the service is stopping.  Tidy up.
            tcpListener.Stop();
        });

        return _listenerTask;
    }

    public void Dispose()
    {
        if (_logger != null)
        {
            Task.WaitAll(_listenerTask);
        }
    }
}

public class Listener<TTelemetryProcessor>(
    IServiceProvider serviceProvider,
    IOptions<ListenerOptions<TTelemetryProcessor>> options,
    ILoggerFactory loggerFactory)
    : Listener(new ServiceProviderReactorFactory(serviceProvider),
        options.Value.IpAddress,
        options.Value.Port,
        loggerFactory.CreateLogger(LoggerCategoryName))
    where TTelemetryProcessor : ITelemetryProcessor
{
    private static readonly string LoggerCategoryName = $"Listener[{typeof(TTelemetryProcessor).Name}]";

    public class ServiceProviderReactorFactory(IServiceProvider serviceProvider) : ITelemetryProcessorFactory
    {
        private static readonly ObjectFactory ActivatorFactory =
            ActivatorUtilities.CreateFactory(typeof(TTelemetryProcessor), [typeof(TcpClient)]);

        public ITelemetryProcessor CreateTelemetryProcessor(TcpClient tcpClient)
        {
            return (ITelemetryProcessor)ActivatorFactory(serviceProvider, [tcpClient]);
        }
    }
}

// ReSharper disable once UnusedTypeParameter - TReactor is to associate this with Listener<TReactor>