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
    private readonly IAlarmService alarmService;
    protected readonly WorkerOptions workerOptions;
    protected string className { get; init; }

    public ServiceWorkerBase(
        ILoggerFactory loggerFactory, 
        IOptions<WorkerOptions> workerOptions, 
        IAlarmService alarmService,
        ITriggerService triggerService)
    {
        this.triggerService = triggerService;
        this.workerOptions = workerOptions.Value;
        this.alarmService = alarmService;
        this.logger = loggerFactory.CreateLogger<ServiceWorkerBase>();
        className = GetType().Name;
    }

    public virtual void Run(CancellationToken stoppingToken)
    {
        logger.LogDebug("starting {0}......", className);
    }

    public async Task<bool> ProcessAlarmsAsync(IList<BreachedTriggerDto> triggersToHandle)
    {
        foreach (var trigger in triggersToHandle)
        {
            using var logScope = logger.BeginScope("{AlarmId} {AlarmType} {SensorId}", trigger.TriggerId, trigger.TriggerType.Code, trigger.SensorId);
            logger.LogDebug("Processing");
            var result= alarmService.ShouldSendAlarmUnlessQuenched(trigger);
            switch (result.Action)
            {
                case SendAlarmAction.Send:
                    var isQuenched = await triggerService.IsQuenchedAsync(trigger);
                    if (!isQuenched)
                    {
                        logger.LogTrace("Sending alarm via {@RecipientMode}", trigger.CommunicationMode);
                        alarmService.SendAlarm(trigger);
                    }

                    triggerService.NoteAlarmTrigger(trigger, !isQuenched);
                    break;
                case SendAlarmAction.DoNotSend:

                    triggerService.NoteAlarmNotTriggered(trigger);
                    break;
                case SendAlarmAction.Skip:
                    break;
            }

            if (trigger.PendingAlarmTriggerId != int.MinValue)
            {
                triggerService.AcknowledgeProcessing(trigger.PendingAlarmTriggerId.Value);
            }
        }

        return triggersToHandle.Count > 0;
    }
}
