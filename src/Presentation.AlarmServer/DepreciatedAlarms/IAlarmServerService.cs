using Application.Alarms.Dtos;
using Application.Triggers;

namespace Presentation.AlarmServer.Alarms;
public interface IAlarmServerService
{
    AlarmSendingResult ShouldSendAlarmUnlessQuenched(BreachedTriggerDto trigger);
    void SendAlarm(BreachedTriggerDto triggerDto);
}