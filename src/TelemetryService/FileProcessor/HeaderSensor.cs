namespace Powelectrics.Telemetry.FileProcessor
{
    public class HeaderSensor
    {
        public int SensorNumber { get; }
        public string Identifier { get; }
        public string SerialNo { get; }

        public HeaderSensor(string sensorNumber, string identifier, string serialNo)
        {
            SensorNumber = int.Parse(sensorNumber);
            Identifier = identifier;
            SerialNo = serialNo;
        }
    }
}
