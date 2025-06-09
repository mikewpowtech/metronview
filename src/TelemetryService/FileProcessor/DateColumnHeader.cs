using System;
using System.Globalization;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class DateColumnHeader : IColumnHeader
    {
        public CsvReadingContext Fill(ReadingRow r, IReading reading, CsvReadingContext readingContext)
        {
            DateTime parsedDate = DateTime.ParseExact(((DateReading)reading).Value, "dd/MM/yyyy", CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AssumeLocal);
            r.DateRecorded = parsedDate.Date.Add(r.DateRecorded.TimeOfDay);
            return readingContext;
        }
    }
}
