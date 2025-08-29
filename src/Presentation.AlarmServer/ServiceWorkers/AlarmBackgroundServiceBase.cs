using Application.Alarms;
using Application.Triggers;
using Domain.Enums;
using Microsoft.Extensions.Options;
using Presentation.AlarmServer.Options;

namespace Presentation.AlarmServer.ServiceWorkers;

public abstract class AlarmBackgroundServiceBase : BackgroundService
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILogger _logger;
    protected readonly WorkerOptions _workerOptions;
    protected readonly string _className;

    protected AlarmBackgroundServiceBase(IServiceProvider serviceProvider, ILogger logger, IOptions<WorkerOptions> workerOptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _workerOptions = workerOptions.Value;
        _className = GetType().Name;
    }

    protected async Task<bool> ProcessAlarmsAsync(IList<BreachedTriggerDto> triggersToHandle)
    {
        using var scope = _serviceProvider.CreateScope();
        var alarmService = scope.ServiceProvider.GetRequiredService<IAlarmService>();
        var triggerService = scope.ServiceProvider.GetRequiredService<ITriggerService>();
        
        foreach (var trigger in triggersToHandle)
        {
            using var logScope = _logger.BeginScope("{AlarmId} {AlarmType} {SensorId}", trigger.TriggerId, trigger.TriggerType.Code, trigger.SensorId);
            _logger.LogDebug("Processing");
            var result = alarmService.ShouldSendAlarmUnlessQuenched(trigger);
            switch (result.Action)
            {
                case SendAlarmAction.Send:
                    var isQuenched = await triggerService.IsQuenchedAsync(trigger);
                    if (!isQuenched)
                    {
                        _logger.LogTrace("Sending alarm via {@RecipientMode}", trigger.CommunicationMode);
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