using System;

namespace Presentation.AlarmServer.Models;

public class Reading
{
    public int SensorId { get; init; }
    public double Value { get; init; }
    public DateTime DateRecordedUtc { get; init; }

    public override string ToString()
    {
            return $"{Value} at {DateRecordedUtc}";
        }
}