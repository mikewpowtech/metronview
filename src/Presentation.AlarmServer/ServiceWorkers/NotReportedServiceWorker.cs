using Application.Alarms;
using Application.Options;
using Application.Triggers;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;

namespace Presentation.AlarmServer.ServiceWorkers;

// us a primary constructor for brevity of code and consistency with other service workers
public class NotReportedServiceWorker(
    ILoggerFactory loggerFactory, 
    IOptions<WorkerOptions> workerOptions,
    //IAlarmServerService depreciatedAlarmService, 
    IAlarmService alarmService, ITriggerService triggerService)
    : ServiceWorkerBase(loggerFactory,workerOptions, alarmService, triggerService
        )
{
    private readonly IAlarmService alarmService = alarmService;

    public override async void Run(CancellationToken cancellationToken)
    {
        base.Run(cancellationToken);
        if (workerOptions.NotReportedPollInterval == 0) { logger.LogTrace("NotReportedPollInterval=0, Exit Run()"); }
        else
        {
            //var st=await alarmService.GetAllAsync();
            //logger.LogTrace("got alarm count {AlarmCount}", st.Count);
            logger.LogTrace("Enter Run()");
            while (!cancellationToken.IsCancellationRequested)
                try
                {
                    logger.LogDebug("Processing");
                    var alarmsToHandle = await triggerService.GetNotReportedBreachesAsync();
                    //var alarmsToHandle = telemetryDatabase.GetNotReportedReadingsWithAlarms();
                    await ProcessAlarmsAsync(alarmsToHandle);
                    cancellationToken.WaitHandle.WaitOne(workerOptions.NotReportedPollInterval);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Exception bubbled up to NotReportedPoller.Run()");
                    // Force a short delay between tries so that we don't get thousands of errors per second from e.g. a database down issue.
                    cancellationToken.WaitHandle.WaitOne(Math.Max(workerOptions.ErrorPauseInterval, workerOptions.NotReportedPollInterval));
                }

            logger.LogTrace("Exit Run()");
        }
    }
}