namespace Powelectrics.Telemetry.FileProcessor
{
    public class DoubleReading : IReading
    {
        public double Value { get; }

        public DoubleReading(double value)
        {
            Value = value;
        }
    }
}
