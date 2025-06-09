using System;
using System.Globalization;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class TimeColumnHeader : IColumnHeader
    {
        public CsvReadingContext Fill(ReadingRow r, IReading reading, CsvReadingContext readingContext)
        {
            DateTime parsedTime = DateTime.ParseExact(((TimeReading)reading).Value, "HH:mm", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AssumeLocal);
            r.DateRecorded = r.DateRecorded.Date.Add(parsedTime.TimeOfDay);
            return readingContext;
        }
    }
}
