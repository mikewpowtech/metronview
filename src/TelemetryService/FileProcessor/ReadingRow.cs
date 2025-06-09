using System;
using System.Collections.Generic;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class ReadingRow
    {
        public int RecordNumber { get; set; }
        public DateTime DateRecorded { get; set; }
        public Dictionary<int, List<double>> ValuesBySensorNo { get; }

        public ReadingRow()
        {
            DateRecorded = DateTime.MinValue;
            ValuesBySensorNo = new Dictionary<int, List<double>>();
        }
    }
}
