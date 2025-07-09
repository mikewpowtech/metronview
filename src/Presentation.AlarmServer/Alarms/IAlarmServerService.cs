using Presentation.AlarmServer.Models;

namespace Presentation.AlarmServer.Alarms;
public interface IAlarmServerService
{
    AlarmSendingResult ShouldSendAlarmUnlessQuenched(AlarmServerDto trigger);
    void SendAlarm(AlarmServerDto triggerDto);
}