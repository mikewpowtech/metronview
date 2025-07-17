using Domain.Enums;

namespace Application.Alarms.Dtos;

public record AlarmSendingResult(SendAlarmAction Action)
{
    //TODO: I don't like this let's find something a bit better 
    public static implicit operator AlarmSendingResult(bool value)
    {
        return value
            ? new AlarmSendingResult(SendAlarmAction.Send)
            : new AlarmSendingResult(SendAlarmAction.DoNotSend);
    }
}
