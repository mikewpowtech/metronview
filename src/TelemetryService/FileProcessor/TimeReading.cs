namespace Powelectrics.Telemetry.FileProcessor
{
    public class TimeReading : IReading
    {
        public string Value { get; }

        public TimeReading(string value)
        {
            Value = value;
        }
    }
}
