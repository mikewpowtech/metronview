using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Antlr4.Runtime;
using NLog;

namespace Powelectrics.Telemetry.FileProcessor
{
    /// <summary>
    /// Runs on a background thread to look for files that need processing, wait a while to ensure (as far as possible) that they've arrived completely, and process them.
    /// </summary>
    class FilePoller
    {
        private static readonly Logger nLogger = LogManager.GetCurrentClassLogger();
        private static readonly Queue<FileAndTimestamp> fileQueue = new Queue<FileAndTimestamp>();
        private static readonly ISet<string> knownPaths = new HashSet<string>();

        /// <summary>
        /// Minimum period between a file being found and it being processed.  Tune as necessary so that this does not unnecessarily delay file processing but the probability of a file still being open for writing at the time of processing is sufficiently low that retries are rare.
        /// </summary>
        private const int FILE_WRITE_CAUTION_INTERVAL = 10; // seconds

        public void Poll()
        {
            bool mightBeFiles = true;
            while (mightBeFiles)
            {
                try
                {
                    FindNewFiles();
                    mightBeFiles = ProcessOldFiles();
                }
                catch (Exception ex)
                {
                    nLogger.Error(ex, "Exception while detecting or processing files");
                    mightBeFiles = false;
                }
            }
        }

        private static bool ProcessOldFiles()
        {
            DateTime newestAllowed = DateTime.UtcNow.AddSeconds(0 - FILE_WRITE_CAUTION_INTERVAL);
            bool atLeastOneProcessed = false;
            while (fileQueue.Count != 0)
            {
                FileAndTimestamp candidate = fileQueue.Peek();
                if (candidate.DateFound > newestAllowed)
                    break;

                // Whether or not the file processes successfully, there's nothing we can do to change the outcome.  Remove it from consideration before processing, therefore.
                knownPaths.Remove(candidate.Path);
                fileQueue.Dequeue();

                Process(candidate.Path);
                atLeastOneProcessed = true;
            }
            return atLeastOneProcessed;
        }

        private static void Process(string path)
        {
            try
            {
                TubeCubeDump dump = ParseOrFail(path);
                bool succeeded = null != dump;
                if (succeeded)
                    succeeded &= MaybeSave(dump, path);
                MoveOrDelete(path, succeeded);
            }
            catch (FileInUseException)
            {
                // Expected; swallow it.  The file won't be moved, and we'll pick it up and put it back in the queue for later retry.
            }
        }

        private static void MoveOrDelete(string path, bool succeeded)
        {
            string movePath = ConfigurationManager.AppSettings[succeeded ? "SuccessLocation" : "FailureLocation"];
            movePath = Path.Combine(movePath, Path.GetFileName(path));
            // Clear out any old file at the target location.  This doesn't throw any exception if the file doesn't exist, so is safe to use without checking File.Exists().
            File.Delete(movePath);
            File.Move(path, movePath);
        }

        private static bool MaybeSave(TubeCubeDump dump, string path)
        {
            if (null == dump)
            {
                nLogger.Error("Parse failed, no error message from {0}", path);
                return false;
            }
            if (null != dump.ParseErrors)
            {
                nLogger.Error("Parse failed: {0}", dump.ParseErrors);
                return false;
            }
            // If there are no rows, succeed in order to move the file away - sometimes they just come in empty.
            if (null != dump.Header && null == dump.Header.HeaderSensors  && null == dump.Readings)
                return true;

            // If there isn't enough information to tell us what the sensors are, fail now.
            if (null == dump.Header || null == dump.Header.HeaderSensors || dump.Header.HeaderSensors.Count < 1)
            {
                nLogger.Error("No header or no probes described from unit {0} in {1}", dump.IdFromFilename, path);
                return false;
            }
            if (null == dump.Readings || null == dump.Readings.Rows)
            {
                nLogger.Error("No rows from unit {0} in {1}", dump.IdFromFilename, path);
                return false;
            }

            // If we get here, we're good to save.
            return Save(dump);
        }

        private static bool Save(TubeCubeDump dump)
        {
            using SqlConnection cn = Manager.GetSqlConnection();
            foreach (HeaderSensor hs in dump.Header.HeaderSensors)
            {
                string serialNo = hs.SerialNo;
                // The built-in barometer is considered a probe, but always has the special serial number "10101010" - an especially glorious hack on the manufacturer's part!
                // We associate the built-in sensors with the device itself, hence this approach.
                if ("10101010".Equals(serialNo))
                    serialNo = dump.IdFromFilename;
                int rtuId = LookupRtu(cn, serialNo);
                if (rtuId < 0)
                {
                    nLogger.Error("No RTU entry for unit/probe '{0}'", hs.SerialNo);
                }
                else
                {
                    SaveStatus(cn, rtuId, dump.Status.Temperature.Value, dump.Status.GsmLevel.Value, dump.DateFromFilename == DateTime.MinValue ? dump.FileCreationDateUtc : dump.DateFromFilename);
                    DateTime previousDateRecorded = DateTime.MinValue;
                    foreach (ReadingRow r in dump.Readings.Rows)
                    {
                        if (r.DateRecorded != previousDateRecorded && r.ValuesBySensorNo.ContainsKey(hs.SensorNumber))
                            SaveReadings(cn, rtuId, dump.FileCreationDateUtc, r.DateRecorded, r.ValuesBySensorNo[hs.SensorNumber]);
                        previousDateRecorded = r.DateRecorded;
                    }
                }
            }
            // If we get here without exception or return, we're done.
            return true;
        }

        private static void SaveReadings(SqlConnection cn, int rtuId, DateTime dateReceivedUtc, DateTime dateRecordedUtc, List<double> readings)
        {
            for (int i = 0; i < readings.Count; i++)
            {
                int channel = i + 1;
                double value = readings[i];
                using SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AddReading";
                cmd.Parameters.AddWithValue("@rtuId", rtuId);
                cmd.Parameters.AddWithValue("@channel", channel);
                cmd.Parameters.AddWithValue("@dateReceivedUtc", dateReceivedUtc);
                cmd.Parameters.AddWithValue("@dateRecordedUtc", dateRecordedUtc);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@isAlarm", false);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                    {
                        // Duplicate key; we may be re-processing an existing file.  Informational.
                        nLogger.Info("Duplicate key in AddReading", ex);
                    }
                    else
                        throw;
                }
            }
        }

        private static void SaveStatus(SqlConnection cn, int rtuId, double temperature, double signal, DateTime dateReceived)
        {
            using SqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AddStatus";
            cmd.Parameters.AddWithValue("@rtuId", rtuId);
            cmd.Parameters.AddWithValue("@mip", false);
            cmd.Parameters.AddWithValue("@failedCallout", false);
            cmd.Parameters.AddWithValue("@temperature", temperature);
            cmd.Parameters.AddWithValue("@battAlarm", false);
            cmd.Parameters.AddWithValue("@autoConfig", false);
            cmd.Parameters.AddWithValue("@carrier", DBNull.Value);
            cmd.Parameters.AddWithValue("@signal", signal);
            // cmd.Parameters.AddWithValue("@dateReceived", dateReceived);
            cmd.ExecuteNonQuery();
        }

        /// <remarks>PRE: id is set to the ID of the client.</remarks>
        private static int LookupRtu(SqlConnection cn, string manufacturerId)
        {
            try
            {
                using SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LookupRtu";
                cmd.Parameters.AddWithValue("@manufacturerId", manufacturerId);
                using SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                    return r.GetInt32(0);
                else
                    return -1;
            }
            catch (SqlException)
            {
                return -1;
            }
        }

        private static TubeCubeDump ParseOrFail(string path)
        {
            try
            {
                using TextReader tr = new StreamReader(path, Encoding.ASCII);
                AntlrInputStream input = new AntlrInputStream(tr);
                TubeCubeLexer lexer = new TubeCubeLexer(input);
                CommonTokenStream tokenStream = new CommonTokenStream(lexer);
                TubeCubeParser parser = new TubeCubeParser(tokenStream);
                StringBuilder errorBuilder = new StringBuilder();
                parser.RemoveErrorListeners();
                parser.AddErrorListener(new AccumulateErrors(errorBuilder));
                TubeCubeParser.CompileUnitContext retval = parser.compileUnit();
                if (parser.NumberOfSyntaxErrors > 0)
                    return new TubeCubeDump() { ParseErrors = errorBuilder.ToString() };
                if (null == retval || null == retval.value)
                    return null;
                Regex filenameParser = new Regex("([0-9]+)__([0-9][0-9]-[A-Z][a-z][a-z][a-z]?-[0-9][0-9][0-9][0-9]__[0-9][0-9]_[0-9][0-9]).csv$");
                Match m = filenameParser.Match(path);
                if (m.Success)
                {
                    retval.value.IdFromFilename = m.Groups[1].Value;
                    string rawDate = m.Groups[2].Value;
                    if (null != rawDate)
                    {
                        rawDate = rawDate.Replace("Sept", "Sep");
                        retval.value.DateFromFilename = DateTime.ParseExact(rawDate, "dd-MMM-yyyy__HH_mm", CultureInfo.GetCultureInfo("en-GB"));
                    }
                }
                retval.value.FileCreationDateUtc = File.GetCreationTimeUtc(path);
                return retval.value;
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("it is being used by another process"))
                    throw new FileInUseException(ex);
                else
                {
                    nLogger.Info(ex, "Parse of '" + path + "' failed");
                    return null;
                }
            }
            catch (Exception ex)
            {
                nLogger.Info(ex, "Parse of '" + path + "' failed");
                return null;
            }
        }

        /// <summary>
        /// Find and enqueue any files in our poll area that exist and have not previously been detected.
        /// </summary>
        private static void FindNewFiles()
        {
            string root = ConfigurationManager.AppSettings["MonitorLocation"];
            foreach (string path in Directory.EnumerateFiles(root))
            {
                if (!knownPaths.Contains(path))
                {
                    knownPaths.Add(path);
                    fileQueue.Enqueue(new FileAndTimestamp { Path = path, DateFound = DateTime.UtcNow });
                }
            }
        }

        private class AccumulateErrors : IAntlrErrorListener<IToken>
        {
            private readonly StringBuilder sb;

            public AccumulateErrors(StringBuilder sb)
            {
                this.sb = sb;
            }

            public void SyntaxError(TextWriter tw, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                sb.AppendLine("Line " + line + ", character " + charPositionInLine + ": " + msg + " (offending symbol was of type " + offendingSymbol.Type + ")");
            }
        }
    }
}
