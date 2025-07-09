using System;

namespace Presentation.AlarmServer.Alarms;

///
/// AlarmType type = AlarmType.Above;
///char code = (char)type; // code == 'A'
/// AlarmType type = AlarmType.Above;
///string name = type.ToString(); // name == "Above"
/// 
public enum AlarmType
{
    Above = 'A',
    Below = 'B',
    RateOfChange = 'C',
    NotReportedForPeriod = 'S',
    ClockReset = 'T',
    Rising = 'U',
    Falling = 'D'
}
