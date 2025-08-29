using Application.Triggers;
using Application.Units;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;

namespace Presentation.AlarmServer.ServiceWorkers;

public class AlarmWorkerService : AlarmBackgroundServiceBase
{
    public AlarmWorkerService(IServiceProvider serviceProvider, ILogger<AlarmWorkerService> logger, IOptions<WorkerOptions> workerOptions)
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
                if (_workerOptions.AlarmPollerInterval == 0)
                {
                    _logger.LogInformation("AlarmPollerInterval=0, Exit ExecuteAsync()");
                    await Task.Delay(_workerOptions.DefaultPauseInterval, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("running {0}....", _className);

                    // Create a new scope for each iteration
                    using var scope = _serviceProvider.CreateScope();
                    var unitService = scope.ServiceProvider.GetRequiredService<IUnitService>();
                    var triggerService = scope.ServiceProvider.GetRequiredService<ITriggerService>();

                    var resp = await unitService.GetAllAsync();
                    _logger.LogInformation("got {0} units", resp.Count);

                    // Use the inherited ProcessAlarmsAsync method
                    var triggers = await triggerService.GetNotReportedBreachesAsync();
                    if (triggers.Count == 0)
                    {
                        _logger.LogInformation("No triggers to process");
                    }
                    else
                    {
                        _logger.LogInformation("got {0} triggers to process", triggers.Count);
                        await ProcessAlarmsAsync(triggers);
                    }

                    await Task.Delay(_workerOptions.AlarmPollerInterval, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception bubbled up to {0}, error {1}", _className, ex.Message);
                // Force a short delay between tries so that we don't get thousands of errors per second from e.g. a database down issue.
                await Task.Delay(_workerOptions.ErrorPauseInterval, stoppingToken);
            }
        }

        _logger.LogTrace("exiting {0}....", _className);
    }
}