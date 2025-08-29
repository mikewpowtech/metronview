using Application.Triggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;

namespace Presentation.AlarmServer.ServiceWorkers;

public class NotReportedAlarmWorkerService : AlarmBackgroundServiceBase
{
    public NotReportedAlarmWorkerService(IServiceProvider serviceProvider,
        ILogger<NotReportedAlarmWorkerService> logger, IOptions<WorkerOptions> workerOptions)
        : base(serviceProvider, logger, workerOptions)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Enter AlarmWorkerService ExecuteAsync interval {_workerOptions.AlarmPollerInterval}");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogDebug("Processing");
                if (_workerOptions.NotReportedPollInterval == 0)
                {
                    _logger.LogInformation("NotReportedPollInterval=0, Exit ExecuteAsync()");
                    await Task.Delay(_workerOptions.DefaultPauseInterval, stoppingToken);
                }
                else
                {
                    using var scope = _serviceProvider.CreateScope();
                    var triggerService = scope.ServiceProvider.GetRequiredService<ITriggerService>();

                    var alarmsToHandle = await triggerService.GetNotReportedBreachesAsync();
                    await ProcessAlarmsAsync(alarmsToHandle);

                    await Task.Delay(_workerOptions.NotReportedPollInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception bubbled up to NotReportedAlarmWorkerService.ExecuteAsync()");
                // Force a short delay between tries so that we don't get thousands of errors per second from e.g. a database down issue.
                await Task.Delay(Math.Max(_workerOptions.ErrorPauseInterval, _workerOptions.NotReportedPollInterval), stoppingToken);
            }
        }

        _logger.LogTrace("Exit ExecuteAsync()");
    }
}