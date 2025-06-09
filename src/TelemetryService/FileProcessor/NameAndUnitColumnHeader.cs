using System.Collections.Generic;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class NameAndUnitColumnHeader : IColumnHeader
    {
        public string Name { get; set; }
        public string Unit { get; set; }

        public CsvReadingContext Fill(ReadingRow r, IReading reading, CsvReadingContext readingContext)
        {
            if (!r.ValuesBySensorNo.ContainsKey(readingContext.CurrentSensorNo))
                r.ValuesBySensorNo.Add(readingContext.CurrentSensorNo, new List<double>());
            r.ValuesBySensorNo[readingContext.CurrentSensorNo].Add(((DoubleReading)reading).Value);
            return readingContext;
        }
    }
}
