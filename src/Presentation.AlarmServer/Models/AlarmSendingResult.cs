using Presentation.AlarmServer.Enums;

namespace Presentation.AlarmServer.Models;

public record AlarmSendingResult(SendAlarmAction Action)
{
    public static implicit operator AlarmSendingResult(bool value)
    {
        return value
            ? new AlarmSendingResult(SendAlarmAction.Send)
            : new AlarmSendingResult(SendAlarmAction.DoNotSend);
    }
}
