using Application.Alarms;
using Application.Options;
using Application.Readings;
using Application.Triggers;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;

namespace Presentation.AlarmServer.ServiceWorkers;

//using a primary constructor
public class AlarmServiceWorker(ILoggerFactory loggerFactory,
                IAlarmService alarmService,ITriggerService triggerService,  
                IOptions<WorkerOptions> workerOptions, IReadingService readingService) 
    : ServiceWorkerBase(loggerFactory, workerOptions, alarmService, triggerService)
{
    private readonly IReadingService readingService = readingService;

    public override async void Run(CancellationToken cancellationToken)
    {
        base.Run(cancellationToken);
        if (workerOptions.AlarmPollerInterval == 0) { logger.LogTrace("{0} workerOptions.AlarmPollerInterval=0, exiting Run()...", className); }
        else
        {
            //logger.LogTrace("Enter Run()");
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    logger.LogTrace("running {0}....", className);
                    await ProcessAlarmsAsync(readingService.GetNewReadingsWithAlarms());
                    cancellationToken.WaitHandle.WaitOne(workerOptions.AlarmPollerInterval);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception bubbled up to {0}, error {1}",className, ex.Message);
                    // Force a short delay between tries so that we don't get thousands of errors per second from e.g. a database down issue.
                    cancellationToken.WaitHandle.WaitOne(workerOptions.ErrorPauseInterval);
                }

            logger.LogTrace("exiting {0}....", className);
        }
    }
}