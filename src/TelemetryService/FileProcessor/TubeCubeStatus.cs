namespace Powelectrics.Telemetry.FileProcessor
{
    public class TubeCubeStatus
    {
        public ValueAndUnit Temperature { get; set; }
        public ValueAndUnit GsmLevel { get; set; }
        public ValueAndUnit BatteryLevel { get; set; }
        public string FirmwareVersion { get; set; }
        public int SystemResets { get; set; }
    }
}
