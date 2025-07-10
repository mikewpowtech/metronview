using System;
using Domain.Enums;

namespace Application.Triggers;

public class BreachedTriggerDto
{
    public int SensorId { get; set; }
    public TriggerTypeCode TriggerTypeCode { get; set; }
    public int TriggerValue { get; set; }
    public int CommunicationModeId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public double? Value { get; set; }
    public bool? IsAlarm { get; set; }
    public DateTime? DateRecordedUtc { get; set; }
    public int? PendingAlarmTriggerId { get; set; }
    public int AlarmSetId { get; set; }
    public int TriggerId { get; set; }
    public int MinimumSendIntervalMinutes { get; set; }
}
