using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Alarms;
using Presentation.AlarmServer.Data.TelemetrySQL;
using Presentation.AlarmServer.Email;
using Presentation.AlarmServer.Options;
using System;
using System.Threading;

namespace Presentation.AlarmServer.ServiceWorkers;

//using a primary constructor
public class AlarmServiceWorker(ITelemetryDatabase telemetryDatabase, ILoggerFactory loggerFactory,
                IAlarmServerService alarmTriggerService,  IOptions<WorkerOptions> workerOptions) 
    : ServiceWorkerBase(telemetryDatabase, loggerFactory, workerOptions, alarmTriggerService)
{
    public override void Run(CancellationToken cancellationToken)
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
                    ProcessAlarms(telemetryDatabase.GetNewReadingsWithAlarms());
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