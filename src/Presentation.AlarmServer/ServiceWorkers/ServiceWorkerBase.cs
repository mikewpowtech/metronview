using Presentation.AlarmServer.Alarms;
using Presentation.AlarmServer.Data.TelemetrySQL;
using Presentation.AlarmServer.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Models;
using Presentation.AlarmServer.Enums;
using System.Threading;
using System.Collections.Generic;

namespace Presentation.AlarmServer.ServiceWorkers;

public abstract class ServiceWorkerBase: IHostedServiceWorker
{
    protected readonly ILogger logger;
    protected readonly ILoggerFactory loggerFactory;
    private readonly IAlarmServerService alarmService;
    protected readonly WorkerOptions workerOptions;
    protected readonly ITelemetryDatabase telemetryDatabase;
    protected string className { get; init; }

    public ServiceWorkerBase(ITelemetryDatabase telemetryDatabase, ILoggerFactory loggerFactory, IOptions<WorkerOptions> workerOptions, 
                IAlarmServerService alarmService)
    {
        this.telemetryDatabase = telemetryDatabase;
        this.loggerFactory = loggerFactory;
        this.alarmService = alarmService;
        this.workerOptions = workerOptions.Value;
        logger = loggerFactory.CreateLogger<ServiceWorkerBase>();
        className = GetType().Name;
    }

    public virtual void Run(CancellationToken stoppingToken)
    {
        logger.LogDebug("starting {0}......", className);
    }

    public bool ProcessAlarms(IList<AlarmServerDto> alarmsToHandle)
    {
        foreach (var alarm in alarmsToHandle)
        {
            using var logScope = logger.BeginScope("{AlarmId} {AlarmType}  {SensorId}", alarm.AlarmId, alarm.AlarmType, alarm.SensorId);
            logger.LogDebug("Processing");
            switch (alarmService.ShouldSendAlarmUnlessQuenched(alarm).Action)
            {
                case SendAlarmAction.Send:
                    var isQuenched = telemetryDatabase.IsQuenched(alarm);
                    if (!isQuenched)
                    {
                        logger.LogTrace("Sending alarm via {@RecipientMode}", alarm.RecipientMode);
                        alarmService.SendAlarm(alarm);
                    }

                    telemetryDatabase.NoteAlarmTrigger(alarm, !isQuenched);
                    break;
                case SendAlarmAction.DoNotSend:

                    telemetryDatabase.NoteAlarmNotTriggered(alarm);
                    break;
                case SendAlarmAction.Skip:
                    break;
            }

            if (alarm.PendingAlarmTriggerId != int.MinValue)
            {
                telemetryDatabase.AcknowledgeProcessing(alarm.PendingAlarmTriggerId);
            }
        }

        return alarmsToHandle.Count > 0;
    }
}
