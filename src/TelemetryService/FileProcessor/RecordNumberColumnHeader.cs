using System;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class RecordNumberColumnHeader : IColumnHeader
    {
        public CsvReadingContext Fill(ReadingRow r, IReading reading, CsvReadingContext readingContext)
        {
            if (!(reading is DoubleReading))
                throw new Exception("Non-numeric record number reading");
            r.RecordNumber = (int)((DoubleReading)reading).Value;
            return readingContext;
        }
    }
}
