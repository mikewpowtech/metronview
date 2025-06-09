using System;
using System.Collections.Generic;
using System.Globalization;

namespace Powelectrics.Telemetry.FileProcessor
{
    partial class TubeCubeParser
    {
        private List<IColumnHeader> csvHeaderRow;
        private List<IReading> csvReadingRow;

        /// <summary>
        /// Accept a date in the format dd/mm/yyyy or dd-MMM-yyyy and a time in the form hh:mm or hh:mm:ss.  Return the corresponding date, presently in the local timezone.
        /// </summary>
        /// <param name="datePart"></param>
        /// <param name="timePart"></param>
        /// <returns></returns>
        private DateTime MakeDate(string datePart, string timePart)
        {
            // HACK: The units send "Sept" for September, which is not recognised.  If found in the date part, shorten to "Sep".
            datePart = datePart.Replace("Sept", "Sep");
            DateTime parsed;
            if (!DateTime.TryParse(datePart + " " + timePart, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal, out parsed))
                throw new Exception ("Couldn't parse date and time " + datePart + " " + timePart);
            return parsed;
        }

        /// <summary>
        /// A grossly hacky function that takes the headers in csvHeaderRow and the data in csvReadingRow and constructs a split-out set of readings from them.
        /// A better approach wouldn't use class variables to accumulate and pass this stuff around.
        /// </summary>
        /// <returns></returns>
        private ReadingRow MakeReadingRow()
        {
            ReadingRow r = new ReadingRow();
            CsvReadingContext readingContext = new CsvReadingContext();
            for (int i = 0; i < Math.Min(csvHeaderRow.Count, csvReadingRow.Count); i++)
                readingContext = csvHeaderRow[i].Fill(r, csvReadingRow[i], readingContext);
            return r;
        }
    }
}
