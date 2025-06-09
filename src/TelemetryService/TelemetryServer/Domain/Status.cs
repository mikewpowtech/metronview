using System;

namespace TelemetryServer.Domain;

public class Status
{
    public bool Mip { get; set; }
    public bool FailedCallout { get; set; }
    public double Temperature { get; set; }
    public bool BattAlarm { get; set; }
    public bool AutoConfig { get; set; }
    public string Carrier { get; set; }
    public double Signal { get; set; }
    public DateTime DateRecordedUtc { get; set; }
}