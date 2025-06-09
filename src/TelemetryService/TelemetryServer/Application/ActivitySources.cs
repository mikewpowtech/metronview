using System.Diagnostics;

namespace TelemetryServer.Application;

public static class ActivitySources
{
    public static readonly ActivitySource Metron2Receive = new("Telemetry.Metron2/Receive");
    public static ActivitySource Metron2Transaction { get; } = new("Telemetry.Metron2/Transaction");
}