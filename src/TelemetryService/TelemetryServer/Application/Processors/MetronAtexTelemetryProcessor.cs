using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Utilities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TelemetryServer.Infrastructure;
using TelemetryServer.Domain;

namespace TelemetryServer.Application.Reactors
{
    /// <summary>
    /// React to something that's a MetronAtex on the far end, talking using Powelectrics' Bulk protocol.
    /// </summary>
    /// <remarks>All methods in this Reactor must be callable from any thread.  Concurrent read and write notifications might happen and the Reactor must be proof against those.</remarks>
    class MetronAtexTelemetryProcessor : ITelemetryProcessor
    {
        private SqlHelper Sql { get; }
        public static string ConfigurationSection { get; } = "MetronAtex";


        private readonly ILogger nLogger = NullLogger.Instance;

        private const int READ_TIMEOUT = 5; // milliseconds
        /// <summary>
        /// How long will we wait after sending a kill before closing the TCP connection?
        /// </summary>
        private const int KILL_WAIT_TIME_MS = 10000; // milliseconds
        /// <summary>
        /// How long will we wait for fresh input before ending the session?
        /// </summary>
        private const int WATCHDOG_TIMEOUT_MS = 60000; // milliseconds
        /// <summary>
        /// How far does a unit's real-time clock need to drift from our RTC before we'll consider a RTC time set?
        /// </summary>
        private const int MAX_ALLOWED_RTC_DRIFT_S = 300; // seconds
        /// <summary>
        /// The largest date that will still be considered a RTC reset.  The RTC resets to 2006-01-01; as we're not using this code in 2006, assume anything within a month of that is a reset.
        /// </summary>
        private static readonly DateTime MAX_RTC_FOR_RESET = new(2006, 01, 31);
        /// <summary>
        /// Convenience array for string.Split(), when splitting lists on commas.
        /// </summary>
        private static readonly char[] COMMA_ARRAY = { ',' };

        /// <summary>
        /// Convenience array for string.Splie(), when splitting lists on semicolons.
        /// </summary>
        private static readonly char[] SEMICOLON_ARRAY = { ';' };

        // Known data about the client
        private string manufacturerId;
        private int rtuId;
        // private string pin;
        private string secret;
        private bool headerSeen;

        // Known data about the network
        private NetworkStream networkStream;
        private MetronAtexBulkReactorState state;
        private StringBuilder readBuilder;
        /// <summary>
        /// Once a connection has been killed, this timer allows it to wait a while before being terminated.
        /// </summary>
        private Timer killWait;
        /// <summary>
        /// This timer ensures that if a response is not received within a reasonable time, the connection closes down rather than persisting forever.
        /// </summary>
        private Timer watchdog;
        private readonly Encoding encoding;
        /// <summary>
        /// A queue of lines that are not connection control (End block, Acknowledge, Kill or Error) that are in the current transaction.
        /// </summary>
        private readonly IList<string> transactionInputQueue;
        /// <summary>
        /// A queue of lines that will be output to the unit if the current transaction succeeds.
        /// </summary>
        private readonly IList<string> transactionOutputQueue;
        /// <summary>
        /// A stream to hold output bytes so that packets are combined as effectively as possible.
        /// This is necessary because we're using NODELAY on the socket to force it to send bytes when we want it to.
        /// </summary>
        private MemoryStream queuedBytes;
        /// <summary>
        /// A scrap object on which we can lock, to prevent lock(this) calls which can be hijacked by external agents to cause deadlock.
        /// </summary>
        private readonly object lockTarget;
        /// <summary>
        /// If true, reading lines that come in are restamped before being written to the database as their timestamp is dodgy.
        /// </summary>
        private bool restampingReadingsAfterRtcReset;
        /// <summary>
        /// The status is set in multiple places; ensure that we have one available.
        /// </summary>
        private readonly Status savedStatus;
        private readonly TcpClient tcpClient;

        public MetronAtexTelemetryProcessor(TcpClient tcpClient, ILogger<MetronAtexTelemetryProcessor> logger, SqlHelper sql)
        {
            nLogger = logger;
            Sql = sql;
            nLogger.LogTrace("Create Metron Atex Bulk Reactor");

            // Force NODELAY so that we have control over when we send data.
            tcpClient.NoDelay = true;

            // At present, the Powelectrics bulk protocol is ASCII-encoded.  Here's the single place to change that if the over-the-wire encoding ever changes.
            encoding = Encoding.ASCII;
            readBuilder = new StringBuilder();
            transactionInputQueue = new List<string>();
            transactionOutputQueue = new List<string>();
            queuedBytes = new MemoryStream();
            savedStatus = new Status { DateRecordedUtc = DateTime.MinValue };
            SetState(MetronAtexBulkReactorState.ExpectingTransaction);
            this.tcpClient = tcpClient;
            lockTarget = new object();
        }

        public async Task ProcessTelemetryAsync(CancellationToken stoppingToken)
        {
            watchdog = new Timer(WatchdogBark, null, WATCHDOG_TIMEOUT_MS, Timeout.Infinite);
            networkStream = tcpClient.GetStream();
            networkStream.ReadTimeout = READ_TIMEOUT;
            while (!stoppingToken.IsCancellationRequested)
            {
                // If the client's closed, avoid checking.  The reactor is in the process of exiting.
                // TODO: Prevent CPU-intensive spins here!
                if (null != tcpClient)
                {
                    // Read at least one byte, or more if more data is available.
                    int messageSize = Math.Max(tcpClient.Available, 1);
                    byte[] telemetryMessage = new byte[messageSize];
                    int bytesRead = await networkStream.ReadAsync(telemetryMessage, 0, messageSize);
                    try
                    {
                        // Ensure nothing else tries to change the state of the reactor while we're using it
                        lock (lockTarget)
                        {
                            if (null != networkStream)
                            {
                                // Find out how many bytes we read.  Could be smaller than the buffer size in the event of a timeout; could even be zero.
                                if (bytesRead > 0)
                                {
                                    ProcessTelemetryMessage(telemetryMessage, bytesRead);
                                }
                                // If the far end has gone away, stop.  This is a workaround, as asking whether the socket is connected always returns true even if the remote end closes the connection.
                                // Poll(_, SelectRead) returns true if there's an event related to reading.  If we read 0 bytes, it can't be data arriving... so it must be the far end closing.
                                if (0 == bytesRead && tcpClient.Client.Poll(1, SelectMode.SelectRead))
                                {
                                    nLogger.LogDebug("Far end has gone. Closing.");
                                    EndConnection();
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Expect some socket exceptions
                        bool expected = false;
                        if (ex is IOException && ex.InnerException is SocketException)
                        {
                            expected = state == MetronAtexBulkReactorState.Killing || state == MetronAtexBulkReactorState.WaitingAfterKill || state == MetronAtexBulkReactorState.Closed;
                        }
                        else if (ex is SqlException sex && sex.Number == 2627)
                        {
                            // Primary key violation in AddReading, looks OK
                            expected = true;
                        }

                        if (!expected)
                        {
                            nLogger.LogWarning(ex, "Issue in SomeBytesReadOrTimeout");
                            EndConnection();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The bytes in readBuffer[0] to readBuffer[bytesRead - 1] have not previously been processed.  Interpret them.
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <param name="telemetryMessageSize"></param>
        protected virtual void ProcessTelemetryMessage(byte[] telemetryMessage, int telemetryMessageSize)
        {
            // We must prevent anything else from running our state machine concurrently.  This is already done by the lock in SomeBytesReadOrTimeout(), which calls this.
            int offset = 0;
            while (offset < telemetryMessageSize)
            {
                byte b = telemetryMessage[offset];
                char c = (char)b; // A hack, but we've said ASCII so it's acceptable.
                switch (c)
                {
                    case '\r':
                        // Ignore double newlines, so ignore this.
                        break;
                    case '\n':
                        // We've received a newline.  What's in our buffer?
                        string line = readBuilder.ToString();
                        InterpretOrQueueLine(line);

                        // Clear out ready for the next line.
                        readBuilder = new StringBuilder();

                        break;
                    default:
                        readBuilder.Append(c);
                        break;
                }
                offset++;

            }
        }

        /// <summary>
        /// A line has been received - it might be part of a larger block, and we may not be at the end of the block.  Deal with the line.
        /// </summary>
        /// <param name="line"></param>
        protected virtual void InterpretOrQueueLine(string line)
        {
            nLogger.LogDebug("< {0}", line);
            // Check for things that would cause a block to be interpreted.  These are:
            // End of transaction ("E" by itself) - tell the state machine.
            // Acknowledgement of a block ("A" by itself) - tell the state machine.
            // An error ("Err,___" by itself) - give up immediately.

            if ("A".Equals(line))
            {
                if (transactionInputQueue.Count > 0)
                {
                    // Acknowledgement incoming with some previous data that isn't part of a transaction - protocol error.
                    nLogger.LogWarning("Protocol error: received 'A' while transaction input queue not empty");
                    EndConnection();
                }
                else
                {
                    // Interpret the ACK and carry on going.
                    InterpretLine(line);
                }
            }
            else if ("E".Equals(line))
            {
                InterpretTransaction();
                transactionInputQueue.Clear();
            }
            else
            {
                // Something else
                transactionInputQueue.Add(line);
            }
            PacifyTheWatchdog();
        }

        void PacifyTheWatchdog()
        {
            if (null != watchdog)
            {
                watchdog.Change(WATCHDOG_TIMEOUT_MS, Timeout.Infinite);
            }
        }

        /// <summary>
        /// The transaction queue contains an entire transaction, and there was an E at the end.  Process it.
        /// </summary>
        protected virtual void InterpretTransaction()
        {
            // Interpret all lines in transaction
            foreach (string line in transactionInputQueue)
                InterpretLine(line);

            // If there was an error, complete.
            if (MetronAtexBulkReactorState.Closed == state)
            {
                nLogger.LogDebug("InterpretTransation: State was closed after processing lines.");
                return;
            }
            // If we get here, the block was valid.
            // Interpreting may have queued up responses (including their own 'A's). Flush them.
            MaybeSendTransaction();

            // We are done with this Atex transaction. Send final 'A'ck.
            QueueLine("A");
            SendQueuedBytes();

            // TODO: If there's more incoming data, wait for it.
            nLogger.LogTrace("Interpret Transaction completing.");
            if (state != MetronAtexBulkReactorState.ExpectingAck)
                KillConnection();
        }

        protected void MaybeSendTransaction()
        {
            lock (lockTarget)
            {
                nLogger.LogTrace("MaybeSendTransaction: queue count: {0}", transactionOutputQueue.Count);
                if (transactionOutputQueue.Count > 0)
                {
                    // TODO: Break large blocks into 1k pieces
                    foreach (string line in transactionOutputQueue)
                        QueueLine(line);
                    transactionOutputQueue.Clear();
                    nLogger.LogTrace("Transaction complete.");
                    SendQueuedBytes();
                }
            }
        }

        /// <summary>
        /// A single line has been received - it might be part of a larger block.  Deal with that line.
        /// </summary>
        /// <param name="line"></param>
        /// <remarks>This maintains a state machine in order to know where the conversation with the unit is up to.</remarks>
        protected virtual void InterpretLine(string line)
        {
            switch (state)
            {
                case MetronAtexBulkReactorState.ExpectingTransaction:
                    if (line.StartsWith("H"))
                        HandleHeader(line);
                    else if (headerSeen)
                    {
                        if (line.StartsWith("L"))
                            HandleReadings(line);
                        else
                            ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                    }
                    break;
                case MetronAtexBulkReactorState.ExpectingAck:
                    HandleAck(line);
                    break;
                case MetronAtexBulkReactorState.WaitingAfterKill:
                    // We should never get any input after a kill.  If we do, log it (it's a protocol error) and ignore it.
                    nLogger.LogInformation("Unexpected input after a kill: {Line}", line);
                    break;
                case MetronAtexBulkReactorState.Closed:
                    // Do nothing
                    break;
                default:
                    IllegalState();
                    break;
            }
        }

        /// <summary>
        /// We've realised that we need to end the conversation.  Send a kill to the unit, then wait for a while (which apparently gives a more reliable kill).
        /// </summary>
        protected void KillConnection()
        {
            SetState(MetronAtexBulkReactorState.Killing);
            //WriteLineImmediately("K"); // No kill command on Atex. So just set reactor states.
            killWait = new Timer(KillTimeout, null, KILL_WAIT_TIME_MS, Timeout.Infinite);
            SetState(MetronAtexBulkReactorState.WaitingAfterKill);
        }

        /// <summary>
        /// In the case of a CRC error, this handles the CRC error; so the caller doesn't have to.
        /// </summary>
        /// <param name="line"></param>
        /// <returns>false if this handled the line (a CRC error), true if the caller still needs to handle it.</returns>
        protected virtual bool CheckCrc(string line)
        {
            // Check the CRC
            if (!Crc.VerifyCrc(line, secret))
            {
                ProtocolError(HostErrorCode.InvalidSecurityToken);
                return false;
            }

            // If we get here, the CRC is valid and it's not a common case.
            return true;
        }

        /// <summary>
        /// A line's come in that we expect to be a header line.  Deal with it and change state appropriately.
        /// </summary>
        /// <param name="line"></param>
        protected virtual void HandleHeader(string line)
        {
            string[] fields = line.Split(COMMA_ARRAY, StringSplitOptions.None);
            // We should always have exactly 5 fields plus 3 trailing commas:
            // 'H', 'MetronATEX', <rtuid>, <timestamp>, <crc>,,,.  If not, return an error.
            nLogger.LogTrace("n fields is: {0}", fields.Length);
            if (fields.Length != 8)
            {
                ProtocolError(HostErrorCode.IncorrectParametersHeader);
                return;
            }
            // First two fields' content is known - ("H,MetronATEX").
            if (!"H".Equals(fields[0]))
            {
                ProtocolError(HostErrorCode.DeviceTypeNotRecognised);
                return;
            }
            if (!"MetronATEX".Equals(fields[1]))
            {
                ProtocolError(HostErrorCode.DeviceTypeNotRecognised);
                return;
            }
            // If we get here, the header indicates a MetronAtex and has the right number of fields.

            // Try to retrieve this unit's info from the database.
            manufacturerId = fields[2];
            if (!LookupRtuOrFail())
            {
                ProtocolError(HostErrorCode.SerialNumberNotRecognised);
                return;
            }
            // If we get here, we have the RTU ID, its PIN and its secret.

            // Check the CRC claimed, against the serial_number + date text.
            string message = fields[2] + fields[3];
            string claimedCrc = fields[4];
            if (!Crc.VerifyCrcCanonical(message, claimedCrc, secret))
            {
                ProtocolError(HostErrorCode.InvalidSecurityToken);
                return;
            }
            // If we get here, the CRC matches - the unit appears to be who it says it is.

            // Check the timestamp; if it's drifted by more than our maximum allowed drift, correct it.
            DateTime timestamp = ParseTimestamp(fields[3]);
            if (!IsAcceptableTimestamp(timestamp))
            {
                // Log an RTC that's reports a date-time close to its factory reset value, signifying a power reset.
                if (timestamp <= MAX_RTC_FOR_RESET)
                {
                    AddRtcReset(timestamp);
                }

                // Timestamps on any readings we receive will be dodgy.  Ignore them!
                restampingReadingsAfterRtcReset = true;

                QueueSetRealTimeClock();
            }

            // If we get here, there are no issues with the header.
            headerSeen = true;

            nLogger.LogTrace("Header done");
        }

        /// <summary>
        /// Returns the parsed UTC time from a Powelectrics-formatted date and time string, or DateTime.MinValue on a parse error.
        /// Units are always set in UTC.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected DateTime ParseTimestamp(string t)
        {
            nLogger.LogTrace("ParseTimeStamp - in: {0}", t);

            //Atex sends datetimes in two formats. Identify what format to use.
            string dateTimeFormat;
            switch (t.Length)
            {
                case 12:
                    dateTimeFormat = "yyMMddHHmmss";
                    nLogger.LogTrace("is a 12 digit date-time. Using format {0}", dateTimeFormat);
                    break;
                case 14:
                    dateTimeFormat = "yyyyMMddHHmmss";
                    nLogger.LogTrace("is a 14 digit date-time. Using format {0}", dateTimeFormat);
                    break;
                default:
                    nLogger.LogTrace("Date-Time received ({0}) is wrong length ({1})", t, t.Length);
                    return DateTime.MinValue;
            }

            if (!DateTime.TryParseExact(t, "yyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime timestamp))
            {
                nLogger.LogTrace("Date-Time received is invalid: {0}", t);
                return DateTime.MinValue;
            }
            nLogger.LogTrace("ParseTimeStamp - out: {0}", timestamp);
            return timestamp;
        }

        protected virtual bool IsAcceptableTimestamp(DateTime timestamp)
        {
            nLogger.LogTrace("Check acceptable time for {0} (ticks {1})", timestamp, timestamp.Ticks);
            // Bad parses are never acceptable.
            if (DateTime.MinValue == timestamp)
            {
                nLogger.LogTrace("DateTime value did not parse (was minvalue)");
                return false;
            }

            // How many seconds of drift do we have?
            DateTime now = DateTime.UtcNow;
            nLogger.LogTrace("'Now' is {0} (ticks {1})", now, now.Ticks);
            long difference = (now.Ticks - timestamp.Ticks) / TimeSpan.TicksPerSecond;
            nLogger.LogTrace("Difference from timestamp is {0}", difference);
            return !(Math.Abs(difference) > MAX_ALLOWED_RTC_DRIFT_S);
        }

        private void AddStatus()
        {
            try
            {
                using SqlConnection cn = Sql.GetSqlConnection();
                using SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AddStatusAndReadings";
                cmd.Parameters.AddWithValue("@rtuId", rtuId);
                cmd.Parameters.AddWithValue("@mip", savedStatus.Mip);
                cmd.Parameters.AddWithValue("@failedCallout", savedStatus.FailedCallout);
                cmd.Parameters.AddWithValue("@temperature", double.IsNaN(savedStatus.Temperature) ? DBNull.Value : savedStatus.Temperature);
                cmd.Parameters.AddWithValue("@battAlarm", savedStatus.BattAlarm);
                cmd.Parameters.AddWithValue("@autoConfig", savedStatus.AutoConfig);
                cmd.Parameters.AddWithValue("@carrier", string.IsNullOrEmpty(savedStatus.Carrier) ? DBNull.Value : savedStatus.Carrier);
                cmd.Parameters.AddWithValue("@signal", double.IsNaN(savedStatus.Signal) ? DBNull.Value : savedStatus.Signal);
                cmd.Parameters.AddWithValue("@dateRecordedUtc", savedStatus.DateRecordedUtc == DateTime.MinValue ? DBNull.Value : savedStatus.DateRecordedUtc);
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                PreserveAfterError(new object[] { rtuId, savedStatus }, ex);
            }
        }

        private void AddRtcReset(DateTime timestamp)
        {
            try
            {
                using SqlConnection cn = Sql.GetSqlConnection();
                using SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AddRtcReset";
                cmd.Parameters.AddWithValue("@rtuId", rtuId);
                cmd.Parameters.AddWithValue("@incorrectValueUtc", DateTime.MinValue == timestamp ? new DateTime(1753, 1, 1) : timestamp);
                cmd.Parameters.AddWithValue("@dateRecordedUtc", DateTime.UtcNow);
                cmd.ExecuteNonQuery();
            }
            catch (SqlException sqlexception)
            {
                nLogger.LogWarning(sqlexception, "Failed to log RTC reset");
            }
        }

        protected bool TryParseZeroOne(string zeroOneString, out bool result)
        {
            if ("0".Equals(zeroOneString))
            {
                result = false;
                return true;
            }
            if ("1".Equals(zeroOneString))
            {
                result = true;
                return true;
            }
            result = false;
            return false;
        }

        protected virtual void HandleReadings(string line)
        {
            // ATEX: e.g:  L,,t1,0;t2,0;t3,0;t4,0;t5,0;t6,0;temp,24.50
            //       so:   L,,(<id>,<reading_value>;)+ temp,<temp_reading>[;ss,<signal strength>;net,<carrier>]
            //
            // Check and discard L,,

            nLogger.LogTrace("Handling Readings");

            // 'Status' items vary by Atex firmware revision.
            //  Always temp. Sometimes ss and net.
            //  Accumulate them and report those we find.
            if (!line.StartsWith("L,,"))
            {
                ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                return;
            }
            string readingsString = line.Substring(3);

            // We now have the ';' separated readings (including temp). Split them - fail if none.
            string[] splitReadings = readingsString.Split(SEMICOLON_ARRAY);
            if (splitReadings.Length == 0)
            {
                ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                return;
            }

            DateTime dateReceivedUtc = DateTime.UtcNow;
            foreach (string splitReading in splitReadings)
            {
                // "Be conservative in what you send, liberal in what you accept." - W3C HTML recommendation
                string trimmedReading = splitReading.Trim();
                if (string.IsNullOrEmpty(trimmedReading))
                    continue;

                string[] readingParts = trimmedReading.Split(COMMA_ARRAY);
                // Format should be sensor, value.
                if (readingParts.Length != 2)
                {
                    // Odd format
                    ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                    return;
                }

                // Try to parse the reading; fail if we can't.
                string channelName = readingParts[0];

                if ("temp".Equals(channelName))
                {
                    if (!double.TryParse(readingParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double temp))
                    {
                        nLogger.LogWarning("Could not parse temperature value {0}", readingParts[0]);
                        ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                        return;
                    }
                    savedStatus.Temperature = temp;
                    continue;
                }

                if ("ss".Equals(channelName))
                {
                    if (!double.TryParse(readingParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double ss))
                    {
                        nLogger.LogWarning("Could not parse signal strength {0}", readingParts[1]);
                        ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                        return;
                    }
                    savedStatus.Signal = ss;
                    continue;
                }

                if ("net".Equals(channelName))
                {
                    savedStatus.Carrier = readingParts[1];
                    continue;
                }

                // channelName should be 't'<int>
                if (!channelName.StartsWith("t"))
                {
                    ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                    return;
                }
                string channelNumberString = channelName.Remove(0, 1);
                if (!int.TryParse(channelNumberString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int channel)
                     || !double.TryParse(readingParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    // Can't parse one of the numbers
                    nLogger.LogWarning("Couldn't parse a reading - sensorNo: {0}, value: {1}", channelNumberString, readingParts[1]);
                    ProtocolError(HostErrorCode.IncorrectFormatOrContent);
                    return;
                }

                NumericReading numericReading = new() { Channel = channel, DateRecordedUtc = dateReceivedUtc, Value = value };
                AddReading(numericReading, dateReceivedUtc);
            }
            // We log sensor readings for parts of our status; these are logged with the most recent date recorded, so that they match up with other readings.
            // For Atexes, that's always the date received.
            savedStatus.DateRecordedUtc = dateReceivedUtc;
            AddStatus();
            nLogger.LogTrace("Handling Readings Done.");
        }

        /// <summary>
        /// An acknowledge has come in from the unit, and we were waiting for it.  Handle it.
        /// </summary>
        /// <param name="line"></param>
        protected virtual void HandleAck(string line)
        {
            // If we have more to send, send it.
            if (transactionOutputQueue.Count > 0)
            {
                MaybeSendTransaction();
            }
            else
            {
                // Otherwise, we're always unit-initiated, so the only scenarios where we get an ack with nothing more to do are when we have sent config or RTC.
                // If we've sent RTC, we will presently be ignoring readings.  Ensure we get up-to-date readings by sending PIN,6 to the unit and expecting more readings.
                if (restampingReadingsAfterRtcReset)
                {
                    SetState(MetronAtexBulkReactorState.ExpectingTransaction);
                }
                else
                {
                    // Nothing else to send or expect.  At this point, the only remaining thing is to kill the connection.
                    KillConnection();
                }
            }
        }

        protected virtual void ProtocolError(HostErrorCode errorCode)
        {
            nLogger.LogDebug(">ERR: protocol error: {ErrorCode}", (int)errorCode);
            nLogger.LogDebug("Stack: {Stack}", Environment.StackTrace);
            WriteLineImmediately("Err," + (int)errorCode);
            EndConnection();
        }

        protected virtual void IllegalState()
        {
            // We've got into an illegal state.  Close the connection and give up.
            nLogger.LogError("Illegal state encountered. Closing");
            EndConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>PRE: id is set to the ID of the client.</remarks>
        private bool LookupRtuOrFail()
        {
            try
            {
                using SqlConnection cn = Sql.GetSqlConnection();
                using SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "LookupRtu";
                cmd.Parameters.AddWithValue("@manufacturerId", manufacturerId);
                using SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    rtuId = r.GetInt32(0);
                    // pin = (r.GetString(1)).Trim(); //Atex has 3char pin.
                    secret = r.GetString(2);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Add a reading into SQL Server.
        /// </summary>
        private void AddReading(NumericReading numericReading, DateTime dateReceivedUtc)
        {
            try
            {
                using SqlConnection cn = Sql.GetSqlConnection();
                using SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AddReading";
                cmd.Parameters.AddWithValue("@rtuId", rtuId);
                cmd.Parameters.AddWithValue("@channel", numericReading.Channel);
                cmd.Parameters.AddWithValue("@dateReceivedUtc", dateReceivedUtc);
                cmd.Parameters.AddWithValue("@dateRecordedUtc", numericReading.DateRecordedUtc);
                cmd.Parameters.AddWithValue("@value", numericReading.Value);
                cmd.Parameters.AddWithValue("@isAlarm", numericReading.IsAlarm);
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                PreserveAfterError(new object[] { rtuId, numericReading }, ex);
            }
        }

        private void QueueLine(string line)
        {
            nLogger.LogDebug("> {Line}", line);
            QueueBytes(encoding.GetBytes(line));
            QueueBytes(encoding.GetBytes("\r\n"));
        }

        private void WriteLineImmediately(string line)
        {
            QueueLine(line);
            SendQueuedBytes();
        }

        private void QueueBytes(byte[] bytes)
        {
            queuedBytes.Write(bytes, 0, bytes.Length);
        }

        // prepend pin and put on transaction queue.
        // nb. for Atex, we do not add a crc.

        private void QueueFields(params string[] fields)
        {
            string toSend = string.Join(",", fields);
            nLogger.LogTrace("Queueing for send: {0}", toSend);
            transactionOutputQueue.Add(toSend);
        }

        private void SendQueuedBytes()
        {
            try
            {
                byte[] bytes = queuedBytes.ToArray();
                networkStream.Write(bytes, 0, bytes.Length);
                queuedBytes = new MemoryStream();
            }
            catch (Exception ex)
            {
                // Expect some socket exceptions
                bool expected = false;
                if (ex is IOException && ex.InnerException is SocketException)
                {
                    expected = state == MetronAtexBulkReactorState.Killing || state == MetronAtexBulkReactorState.WaitingAfterKill || state == MetronAtexBulkReactorState.Closed;
                }
                if (!expected)
                {
                    nLogger.LogDebug(ex, "Issue writing bytes");
                    EndConnection();
                }
            }
        }

        /// <summary>
        /// Callback from our timer once we've said goodbye.  If nothing else has closed the connection, we do so at this time.
        /// </summary>
        private void KillTimeout(object scrap)
        {
            nLogger.LogDebug("KillTimeout invoked {0}", state);
            try
            {
                lock (lockTarget)
                {
                    if (state == MetronAtexBulkReactorState.WaitingAfterKill)
                        EndConnection();
                }
            }
            catch (Exception ex)
            {
                nLogger.LogDebug(ex, "Exception caught in KillTimeout");
            }
        }

        /// <summary>
        /// Callback from our watchdog timer.  Nothing has happened for a sufficiently long time that we want to kill this connection.
        /// </summary>
        private void WatchdogBark(object scrap)
        {
            lock (lockTarget)
            {
                nLogger.LogDebug("Watchdog bark");
                EndConnection();
            }
        }

        /// <summary>
        /// The connection has finished, gone into an error state, or done something else that means we're not interested in it any more.
        /// Get rid of it, discarding any unsent data, and ensure this Reactor doesn't stay around.
        /// </summary>
        private void EndConnection()
        {
            nLogger.LogDebug("EndConnection invoked");
            lock (lockTarget)
            {
                if (null != networkStream)
                {
                    networkStream.Close(0);
                    networkStream.Dispose();
                    networkStream = null;
                }
                if (null != tcpClient)
                {
                    tcpClient.Close();
                }
                if (null != killWait)
                {
                    killWait.Dispose();
                    killWait = null;
                }
                if (null != watchdog)
                {
                    watchdog.Dispose();
                    watchdog = null;
                }
                SetState(MetronAtexBulkReactorState.Closed);
            }
        }

        void QueueSetRealTimeClock()
        {
            // All dates and times sent to the RTU are UTC times.
            DateTime now = DateTime.UtcNow;
            QueueFields("SP", "1", now.ToString("yyyyMMddHHmmss"));
        }

        /// <summary>
        /// cmd has failed to run against the SQL database; keep it for later update.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ex"> </param>
        private void PreserveAfterError(object _, SqlException ex)
        {
            nLogger.LogWarning(ex, "SQL command exception");
        }

        /// <summary>
        /// Allowed states for a MetronAtexBulkReactor's finite state machine.
        /// </summary>
        protected enum MetronAtexBulkReactorState
        {
            ExpectingTransaction,
            ExpectingAck,
            Killing,
            WaitingAfterKill,
            Closed
        }

        /// <summary>
        /// Error codes.  The values represent the value returned to the unit.
        /// Ensure this always matches Powelectrics' error definitions.
        /// </summary>
        protected enum HostErrorCode
        {
            HeaderTimeout = 0,
            DeviceTypeNotRecognised = 1,
            SerialNumberNotRecognised = 2,
            InvalidSecurityToken = 3,
            IncorrectDateTimeFormat = 4,
            IncorrectParametersHeader = 9,
            UnexpectedErrorBeforeIdentification = 10,
            UnexpectedErrorBeforeAuthentication = 11,
            IncorrectFormatOrContent = 12
        }

        private void SetState(MetronAtexBulkReactorState newState)
        {
            state = newState;
        }
    }
}
