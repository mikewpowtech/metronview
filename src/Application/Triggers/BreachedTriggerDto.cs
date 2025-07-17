using Domain;

namespace Application.Triggers;

public class BreachedTriggerDto
{
    public int SensorId { get; set; }
    public int TriggerTypeId { get; set; }
    public int TriggerValue { get; set; }
    public int CommunicationModeId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public double Value { get; set; }
    public bool IsAlarm { get; set; }
    public DateTime DateRecordedUtc { get; set; }
    public int? PendingAlarmTriggerId { get; set; }
    public int AlarmSetId { get; set; }
    public int TriggerId { get; set; }
    public int MinimumSendIntervalMinutes { get; set; }

    // Make these properties required (non-nullable)
    public TriggerType TriggerType { get; set; } = null!;
    public CommunicationMode CommunicationMode { get; set; } = null!;

    public bool SendAlarmForNotReported { get; init; }
    public Dictionary<string, object> KnownValues { get; set; } = new();
    public bool KnownValuesSet { get; set; }

    public BreachedTriggerDto InitialiseKnownValues()
    {
        KnownValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
        {
            ["sensorId"] = SensorId,
            ["alarmType"] = TriggerType.Code.ToString(), // Use TriggerType.Code instead of TriggerTypeCode
            ["alarmValue"] = double.IsNaN(Value) ? "NaN" : Value, // Fix the logic - was using IsNaN but storing the boolean
            ["recipientMode"] = CommunicationMode.Code, // No null check needed since it's required
            ["value"] = double.IsNaN(Value) ? "NaN" : Value, // Fix the logic - was using IsNaN but storing the boolean
            ["isAlarm"] = IsAlarm ? "Yes" : "No",
            ["dateRecordedUtc"] = DateRecordedUtc,
            ["nowUtc"] = DateTime.UtcNow
        };
        KnownValuesSet = true; // Set the flag
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

    // Add validation method to ensure required properties are set
    public void ValidateRequiredProperties()
    {
        if (TriggerType == null)
            throw new InvalidOperationException("TriggerType must be set");
        
        if (CommunicationMode == null)
            throw new InvalidOperationException("CommunicationMode must be set");
    }
}
