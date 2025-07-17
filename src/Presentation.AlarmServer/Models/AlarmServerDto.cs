
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Presentation.AlarmServer.Models;

public class AlarmServerDto
{
    public int SensorId { get; init; }
    public TriggerTypeCode AlarmType { get; init; }
    public double AlarmValue { get; init; }
    public AlarmRecipientType RecipientMode { get; init; }
    public string AlarmSubject { get; init; }
    public string AlarmBody { get; init; }
    public double Value { get; init; }
    public bool IsAlarm { get; init; }
    public DateTime DateRecordedUtc { get; init; }
    public int PendingAlarmTriggerId { get; init; }
    public int AlarmSetId { get; init; }
    public int AlarmId { get; init; }
    public int MinimumSendIntervalMinutes { get; init; }
    public bool SendAlarmForNotReported { get; init; }
    public Dictionary<string, object> KnownValues { get; set; }
    public bool KnownValuesSet { get; set; }

    public AlarmServerDto()
    {

    }

    public AlarmServerDto InitialiseKnownValues()
    {
        KnownValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["sensorId"] = SensorId,
            ["alarmType"] = AlarmType.ToString(),
            ["alarmValue"] = double.IsNaN(AlarmValue) ? null : AlarmValue,
            ["recipientMode"] = RecipientMode.ToString(),
            ["value"] = double.IsNaN(Value) ? null : Value,
            ["isAlarm"] = IsAlarm ? "Yes" : "No",
            ["dateRecordedUtc"] = DateRecordedUtc,
            ["nowUtc"] = DateTime.UtcNow
        };
        return this;
    }
    
    public void SetKnownValue(string key, object value)
    {
        if (KnownValues.ContainsKey(key))
        {
            KnownValues[key] = value;
        }
        else
        {
            KnownValues.Add(key, value);
        }
    }
    public bool KnowsValue(string key)
    {
        return KnownValues.ContainsKey(key);
    }
}
