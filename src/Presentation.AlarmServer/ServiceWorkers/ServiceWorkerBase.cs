using Application.Alarms;
using Application.Triggers;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;
using System.Collections.Generic;
using System.Threading;

namespace Presentation.AlarmServer.ServiceWorkers;

public abstract class ServiceWorkerBase: IHostedServiceWorker
{
    protected readonly ILogger<ServiceWorkerBase> logger;
    protected readonly ILoggerFactory loggerFactory;
    private readonly ITriggerService triggerService;

    //private readonly IAlarmServerService depreciatedAlarmService;
    private readonly IAlarmService alarmService;
    protected readonly WorkerOptions workerOptions;
    //protected readonly ITelemetryDatabase telemetryDatabase;
    protected string className { get; init; }

    public ServiceWorkerBase(
        ILoggerFactory loggerFactory, 
        IOptions<WorkerOptions> workerOptions, 
        IAlarmService alarmService,
        ITriggerService triggerService)
    {
        //this.loggerFactory = loggerFactory;
        this.triggerService = triggerService;
        //this.depreciatedAlarmService = depreciatedAlarmService;
        this.workerOptions = workerOptions.Value;
        this.logger = loggerFactory.CreateLogger<ServiceWorkerBase>();
        className = GetType().Name;
    }

    public virtual void Run(CancellationToken stoppingToken)
    {
        logger.LogDebug("starting {0}......", className);
    }

    public bool ProcessAlarms(IList<BreachedTriggerDto> alarmsToHandle)
    {
        foreach (var alarm in alarmsToHandle)
        {
            using var logScope = logger.BeginScope("{AlarmId} {AlarmType}  {SensorId}", alarm.TriggerId, alarm.TriggerType.Code, alarm.SensorId);
            logger.LogDebug("Processing");
            switch (alarmService.ShouldSendAlarmUnlessQuenched(alarm).Action)
            {
                case SendAlarmAction.Send:
                    var isQuenched = triggerService.IsQuenched(alarm);
                    if (!isQuenched)
                    {
                        logger.LogTrace("Sending alarm via {@RecipientMode}", alarm.CommunicationMode);
                        alarmService.SendAlarm(alarm);
                    }

                    triggerService.NoteAlarmTrigger(alarm, !isQuenched);
                    break;
                case SendAlarmAction.DoNotSend:

                    triggerService.NoteAlarmNotTriggered(alarm);
                    break;
                case SendAlarmAction.Skip:
                    break;
            }

            if (alarm.PendingAlarmTriggerId != int.MinValue)
            {
                triggerService.AcknowledgeProcessing(alarm.PendingAlarmTriggerId.Value);
            }
        }

        return alarmsToHandle.Count > 0;
    }
}
