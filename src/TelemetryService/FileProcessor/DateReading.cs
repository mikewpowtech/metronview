namespace Powelectrics.Telemetry.FileProcessor
{
    public class DateReading : IReading
    {
        public string Value { get; }

        public DateReading(string value)
        {
            Value = value;
        }
    }
}
