using Domain;
using Domain.Enums;

public static class CommunicationModeExtensions
{
    public static AlarmRecipientType ToAlarmRecipientType(this CommunicationMode communicationMode)
    {
        return communicationMode.Code.ToUpperInvariant() switch
        {
            "S" => AlarmRecipientType.SMS,
            "E" => AlarmRecipientType.Email,
            "W" => AlarmRecipientType.WebService,
            _ => AlarmRecipientType.Unknown
        };
    }
}
