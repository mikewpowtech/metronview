namespace Powelectrics.Telemetry.FileProcessor
{
    public interface IColumnHeader
    {
        CsvReadingContext Fill(ReadingRow r, IReading reading, CsvReadingContext readingContext);
    }
}
