using Domain.Enums;
using System;

namespace Presentation.AlarmServer.Helpers;

public class RecipientModeHelper
{
    public static AlarmRecipientType FromString(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("ToRecipientMode: Type should never be empty", nameof(value));
        switch (value[0])
        {
            case 'E':
                return AlarmRecipientType.Email;
            case 'S':
                return AlarmRecipientType.SMS;
            case 'W':
                return AlarmRecipientType.SMS;
            default:
                return AlarmRecipientType.Unknown;
        }
    }
}
