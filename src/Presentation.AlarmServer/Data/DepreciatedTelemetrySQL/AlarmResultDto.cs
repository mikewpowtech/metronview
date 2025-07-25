using System;

namespace Presentation.AlarmServer.Data.TelemetrySQL;

public class AlarmResultDto
{
    public int SensorID { get; set; }
    public string AlarmTypeID { get; set; }
    public double AlarmValue { get; set; }
    public int RecipientModeID { get; set; }
    public string AlarmSubject { get; set; }
    public string AlarmBody { get; set; }
    public double? Value { get; set; }
    public bool? IsAlarm { get; set; }
    public DateTime? DateRecordedUtc { get; set; }
    public int? PendingAlarmTriggerID { get; set; }
    public int AlarmSetID { get; set; }
    public int AlarmID { get; set; }
    public int MinimumSendIntervalMinutes { get; set; }
}
