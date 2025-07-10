namespace Domain.Enums;

///
/// AlarmType type = AlarmType.Above;
///char code = (char)type; // code == 'A'
/// AlarmType type = AlarmType.Above;
///string name = type.ToString(); // name == "Above"
/// 
public enum TriggerTypeCode
{
    Above = 'A',
    Below = 'B',
    RateOfChange = 'C',
    NotReportedForPeriod = 'S',
    ClockReset = 'T',
    Rising = 'U',
    Falling = 'D'
}
