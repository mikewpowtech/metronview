using TelemetryServer.Enums;

namespace TelemetryServer.Domain;

class Sensor
{
    public int Channel { get; set; }
    public ChannelType ChannelType { get; set; }
    public string Name { get; set; }
    public string EngineeringUnits { get; set; }
    public int LowValue { get; set; }
    public int HighValue { get; set; }
    public int? LoLoAlarmValue { get; set; }
    public int? LoAlarmValue { get; set; }
    public int? HiAlarmValue { get; set; }
    public int? HiHiAlarmValue { get; set; }
    public int? Hysteresis { get; set; }
    public int? CalloutDelay { get; set; }
}
