namespace Powelectrics.Telemetry.FileProcessor
{
    public class SensorNumberColumnHeader : IColumnHeader
    {
        public CsvReadingContext Fill(ReadingRow r, IReading reading, CsvReadingContext readingContext)
        {
            readingContext.CurrentSensorNo = (int)((DoubleReading)reading).Value;
            return readingContext;
        }
    }
}
