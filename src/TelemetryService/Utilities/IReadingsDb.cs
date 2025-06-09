namespace Utilities;

public interface IReadingsDb
{
    /// <summary>
    /// Add a reading into SQL Server.
    /// </summary>
    void AddReading(NumericReading reading);

    void AddReading(StringReading reading);
}