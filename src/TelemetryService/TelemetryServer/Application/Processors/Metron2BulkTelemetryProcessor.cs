using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Metron2Configuration;
using Metron2Parser;
using Utilities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TelemetryServer.Enums;
using TelemetryServer.Telemetry.Reactors;
using TelemetryServer.Infrastructure;
using TelemetryServer.Domain;
using TelemetryServer.Application.Reactors;

namespace TelemetryServer.Telemetry;



/// <summary>
/// Process messages from something that's a Metron2 on the far end, talking using Powelectrics' Bulk protocol.
/// </summary>
/// <remarks>All methods in this Telemetryprocessor must be callable from any thread.  
/// Concurrent read and write notifications might happen and the Reactor must be proof against those.</remarks>
partial class Metron2BulkTelemetryProcessor : ITelemetryProcessor, IReadingsDb, IDisposable
{
    private SqlHelper Sql { get; }
    private ILogger _logger = NullLogger.Instance;

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

    public static string ConfigurationSection { get; } = "Metron2";

    /// <summary>
    /// The largest date that will still be considered a RTC reset.  The RTC resets to 2006-01-01; as we're not using this code in 2006, assume anything within a month of that is a reset.
    /// </summary>
    private static readonly DateTime MAX_RTC_FOR_RESET = new(2006, 01, 31);
    /// <summary>
    /// Convenience array for string.Split(), when splitting lists on commas.
    /// </summary>
    private static readonly char[] COMMA_ARRAY = { ',' };
    /// <summary>
    /// Convenience array for string.Split(), when splitting lists on percents.
    /// </summary>
    private static readonly char[] PERCENT_ARRAY = { '%' };

    // Known data about the client
    private string manufacturerId;
    private int rtuId;
    private string pin;
    private string secret;
    private bool headerSeen;

    // Known data about the network
    private NetworkStream networkStream;
    private Metron2BulkTelemetryProcessorState state;
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
    /// If true, reading lines that come in are ignored rather than written to the database as their timestamp is dodgy.
    /// This is also used to send a PIN,6 command to the unit once we've reset its clock.
    /// </summary>
    private bool ignoringReadingsAfterRtcReset;

    /// <summary>
    /// If true, the device at the far end supplies and requires CRCs.  If false, it neither supplies nor requires CRCs.
    /// </summary>
    private bool usesCrcs;

    /// <summary>
    /// If this is non-zero, a configuration upload was attempted; an ack indicates the upload was successful, an error or connection close indicates it was unsuccessful.
    /// </summary>
    private int pendingConfigurationUploadId;
    private Status savedStatus;
    private readonly IList<Reading> readings;
    private bool alreadyQueuedRequestConfigAndStatus;
    private TcpClient tcpClient;

    public Metron2BulkTelemetryProcessor(TcpClient tcpClient, ILogger<Metron2BulkTelemetryProcessor> logger, SqlHelper sql)
    {
        Sql = sql;
        _logger = logger;
            
        // Force NODELAY so that we have control over when we send data.
        tcpClient.NoDelay = true;

        // At present, the Powelectrics bulk protocol is ASCII-encoded.  Here's the single place to change that if the over-the-wire encoding ever changes.
        encoding = Encoding.ASCII;
        readBuilder = new StringBuilder();
        transactionInputQueue = new List<string>();
        transactionOutputQueue = new List<string>();
        readings = new List<Reading>();
        queuedBytes = new MemoryStream();
        SetState(Metron2BulkTelemetryProcessorState.ExpectingTransaction);
        this.tcpClient = tcpClient;
        lockTarget = new object();
    }

    //TODO abstract this into a base processor, it's common functionality, remember DRY?
    public async Task ProcessTelemetryAsync(CancellationToken stoppingToken)
    {
        watchdog = new Timer(WatchdogBark, null, WATCHDOG_TIMEOUT_MS, Timeout.Infinite);
        networkStream = tcpClient.GetStream();
        networkStream.ReadTimeout = READ_TIMEOUT;

        using var activityScope = InitialiseActivity();
        // Keep going until the client closes (which is done in EndConnection()).  Don't respect stoppingToken as we want to complete current transactions even if we're stopping.
        while (true)
        {
            // If the client's closed, give up.  The reactor is in the process of exiting.
            if (null == tcpClient) break;
                
            // Read at least one byte, or more if more data is available.
            int messageSize = Math.Max(tcpClient.Available, 1);
            byte[] telemetryMessage = new byte[messageSize];
            int bytesRead = await networkStream.ReadAsync(telemetryMessage, 0, messageSize);
            try
            {
                // Ensure nothing else tries to change the state of the reactor while we're using it
                lock (lockTarget)
                {
                    if (null == networkStream) continue;

                    // If the far end has gone away, stop.  This is a workaround, as asking whether the socket is connected always returns true even if the remote end closes the connection.
                    // Poll(_, SelectRead) returns true if there's an event related to reading.  If we read 0 bytes, it can't be data arriving... so it must be the far end closing.
                    if (0 == bytesRead && tcpClient.Client.Poll(1, SelectMode.SelectRead))
                    {
                        EndConnection();
                        break;
                    }

                    // Find out how many bytes we read.  Could be smaller than the buffer size in the event of a timeout; could even be zero.
                    if (bytesRead > 0)
                    {
                        ProcessTelemetryMessage(telemetryMessage, bytesRead);
                    }

                }
            }
            catch (IOException ex) when (ex.InnerException is SocketException &&
                                         (state & Metron2BulkTelemetryProcessorState.ConnectionErrorIsExpected) != 0)
            {

            }
            catch (SqlException ex) when (ex.Number == 2627)
            {

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected exception, closing connection to Metron");
                UnexpectedError(ex);
                EndConnection();
                break;
            }
        }
    }

    /// <summary>
    /// The bytes in readBuffer[0] to readBuffer[bytesRead - 1] have not previously been processed.  Interpret them.
    /// </summary>
    /// <param name="telemetryMessage"></param>
    /// <param name="messageSize"></param>
    protected virtual void ProcessTelemetryMessage(byte[] telemetryMessage, int messageSize)
    {
        // We must prevent anything else from running our state machine concurrently.  This is already done by the lock in SomeBytesReadOrTimeout(), which calls this.
        int offset = 0;
        while (offset < messageSize)
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
        _logger.LogInformation("< {Line}", line);
        // Check for things that would cause a block to be interpreted.  These are:
        // End of transaction ("E" by itself) - tell the state machine.
        // Acknowledgement of a block ("A" by itself) - tell the state machine.
        // Kill the connection ("K" by itself) - the far end is closing the connection, give up immediately.
        // An error ("Err,___" by itself) - give up immediately.

        if ("K".Equals(line))
        {
            EndConnection();
        }
        else if ("A".Equals(line))
        {
            if (transactionInputQueue.Count > 0)
            {
                // Acknowledgement incoming with some previous data that isn't part of a transaction - protocol error.
                _logger.LogWarning("Protocol error: received 'A' while transaction input queue not empty");
                EndConnection();
            }
            else
            {
                // Interpret the ACK and carry on going.
                ProcessLine(line);
            }
        }
        else if ("E".Equals(line))
        {
            InterpretTransaction();
            transactionInputQueue.Clear();
        }
        else if (line.StartsWith("Err,"))
        {
            // Deal specially with errors where we were expecting an ack - they may be after config sends, for example.
            if ((state & Metron2BulkTelemetryProcessorState.ExpectingAck) != 0)
                HandleError();
            // TODO: Log incoming error
            EndConnection();
        }
        else
        {
            // Something else
            transactionInputQueue.Add(line);
        }
        PacifyTheWatchdog();
    }

    /// <summary>
    /// We were expecting an ack.  We got an error.  Deal with it.
    /// </summary>
    private void HandleError()
    {

        if (0 != pendingConfigurationUploadId)
        {
            // The upload failed
            AddConfigurationUploadOutcome(false);
        }
    }

    /// <summary>
    /// A configuration has been sent and acknowledged in some way - either as a success or as a failure.  Record that fact to prevent the config being sent again.
    /// </summary>
    /// <param name="succeeded">true for success, false for failure</param>
    private void AddConfigurationUploadOutcome(bool succeeded)
    {
        try
        {
            using SqlConnection cn = Sql.GetSqlConnection();
            using SqlCommand cmd = cn.CreateCommand();
            cmd.CommandText = "update ConfigurationUploads set UploadStatusID = @uploadStatusId, DateUploadedUtc = getutcdate() where ConfigurationUploadID = @configurationUploadId";
            cmd.Parameters.AddWithValue("@configurationUploadId", pendingConfigurationUploadId);
            cmd.Parameters.AddWithValue("@uploadStatusId", succeeded ? "S" : "F");
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            _logger.LogWarning(ex, "Couldn't complete AddConfigurationUploadOutcome");
        }
        finally
        {
            // Whether the database update succeeded or failed, we no longer have a pending upload
            pendingConfigurationUploadId = 0;
        }
    }

    /// <summary>
    /// Set our watchdog timeout back to its initial value and start the clock ticking again.
    /// </summary>
    private void PacifyTheWatchdog()
    {
        if (null != watchdog)
            watchdog.Change(WATCHDOG_TIMEOUT_MS, Timeout.Infinite);
    }

    /// <summary>
    /// The transaction queue contains an entire transaction, and there was an E at the end.  Process it.
    /// </summary>
    protected virtual void InterpretTransaction()
    {
        using var _ = StartingToInterpretTransaction();
        // Now's a really good time to reset flags that are set per transaction.
        alreadyQueuedRequestConfigAndStatus = false;

        foreach (string line in transactionInputQueue)
            ProcessLine(line);

        // If there was an error, the connection will be closed and the error will already have been sent.
        if (Metron2BulkTelemetryProcessorState.Closed == state)
            return;

        // The transaction may have included a valid status; if so, record it - but only once!
        if (null != savedStatus)
        {
            AddStatus(savedStatus);
            savedStatus = null;
        }

        // Log any readings that came in
        foreach (var reading in readings)
            reading.Save(this);
                
        readings.Clear();

        // Otherwise, the block was valid.  We can acknowledge it, then send our own response (if any) and an end-block.
        QueueLine("A");
        MaybeSendTransaction();
        SendQueuedBytes();
        // TODO: If there's more incoming data, wait for it.
        if ((state & Metron2BulkTelemetryProcessorState.ExpectingAck) == 0)
            KillConnection();
    }

    /// <summary>
    /// If there's any pending output for the remote unit, send it as a transaction (terminated with "E" on a line by its own) and expect an ack.
    /// If there's no pending output for the remote unit, do nothing and do not change the state.
    /// </summary>
    protected void MaybeSendTransaction()
    {
        lock (lockTarget)
        {
            if (transactionOutputQueue.Count > 0)
            {
                // TODO: Break large blocks into 1k pieces
                foreach (string line in transactionOutputQueue)
                    QueueLine(line);
                transactionOutputQueue.Clear();
                QueueLine("E");
                SendQueuedBytes();
                SetState(Metron2BulkTelemetryProcessorState.ExpectingAck | (alreadyQueuedRequestConfigAndStatus ? Metron2BulkTelemetryProcessorState.AnotherTransactionIsExpected : 0));
            }
        }
    }

    /// <summary>
    /// A single line has been received - it might be part of a larger block.  Deal with that line.
    /// </summary>
    /// <param name="line"></param>
    /// <remarks>This maintains a state machine in order to know where the conversation with the unit is up to.</remarks>
    protected virtual void ProcessLine(string line)
    {
        // Some units intersperse blank lines.  Remove them.
        if (string.IsNullOrWhiteSpace(line))
            return;

        switch (state)
        {
            case Metron2BulkTelemetryProcessorState.ExpectingTransaction:
                // When requesting new readings, the remote unit will ack the request to send.  This case handles the ack.
                if (line.Equals("A"))
                    break;
                if (IsHeader(line))
                    ProcessHeader(line);
                else if (headerSeen)
                {
                    if (line.StartsWith("S"))
                        ProcessStatusLine(line);
                    else if (line[0] == 'L' || line[0] == 'A' || line[0] == 'N')
                        ProcessReadings(line);
                    else
                    {
                        _logger.LogWarning("Cannot handle {Line}", line);
                        ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
                    }
                }
                break;
            case Metron2BulkTelemetryProcessorState.ExpectingAck:
                HandleAck(line);
                break;
            case Metron2BulkTelemetryProcessorState.ExpectingAckThenTransaction:
                HandleAck(line);
                state = Metron2BulkTelemetryProcessorState.ExpectingTransaction;
                break;
            case Metron2BulkTelemetryProcessorState.WaitingAfterKill:
                // We should never get any input after a kill.  If we do, log it (it's a protocol error) and ignore it.
                _logger.LogDebug("Unexpected input after a kill: {Line}", line);
                break;
            case Metron2BulkTelemetryProcessorState.Closed:
                // Do nothing
                break;
            default:
                IllegalState();
                break;
        }
    }

    private static bool IsHeader(string line)
    {
        return line.StartsWith("POWE")
               || line.StartsWith("MM2M");
    }

    /// <summary>
    /// We've realised that we need to end the conversation.  Send a kill to the unit, then wait for a while (which apparently gives a more reliable kill).
    /// </summary>
    protected void KillConnection()
    {
        SetState(Metron2BulkTelemetryProcessorState.Killing);
        WriteLineImmediately("K");
        killWait = new Timer(KillTimeout, null, KILL_WAIT_TIME_MS, Timeout.Infinite);
        SetState(Metron2BulkTelemetryProcessorState.WaitingAfterKill);
    }

    /// <summary>
    /// In the case of a CRC error, this handles the CRC error; so the caller doesn't have to.
    /// </summary>
    /// <param name="line"></param>
    /// <returns>false if this handled the line (a CRC error), true if the CRC was valid and hence the caller should continue to process the line.</returns>
    protected virtual bool CheckCrc(string line)
    {
        // Check the CRC
        if (!VerifyCrc(line, secret))
        {
            ProtocolError(BulkProtocolHostErrorCode.InvalidSecurityToken);
            return false;
        }

        // If we get here, the CRC is valid.
        return true;
    }

    /// <summary>
    /// A line's come in that we expect to be a header line.  Deal with it and change state appropriately.
    /// </summary>
    /// <param name="line"></param>
    protected virtual void ProcessHeader(string line)
    {
        string[] fields = line.Split(COMMA_ARRAY, StringSplitOptions.None);
        // We should always have 4 or 5 fields depending on CRC: type, METRON2AP, rtuid, timestamp[, crc].  If not, return an error.
        if (fields.Length < 4)
        {
            ProtocolError(BulkProtocolHostErrorCode.IncorrectParametersHeader);
            return;
        }
        // First two fields' content is known.  Ensure they're correct.

        if ("POWE".Equals(fields[0]))
        {
            usesCrcs = true;
            if (fields.Length != 5)
            {
                ProtocolError(BulkProtocolHostErrorCode.IncorrectParametersHeader);
                return;
            }
        }
        else if ("MM2M".Equals(fields[0]))
        {
            usesCrcs = false;
            if (fields.Length != 4)
            {
                ProtocolError(BulkProtocolHostErrorCode.IncorrectParametersHeader);
                return;
            }
        }
        else
        {
            ProtocolError(BulkProtocolHostErrorCode.DeviceTypeNotRecognised);
            return;
        }

        if (!"METRON2AP".Equals(fields[1]))
        {
            ProtocolError(BulkProtocolHostErrorCode.DeviceTypeNotRecognised);
            return;
        }
        // If we get here, the header indicates a Metron2 or MM2M PLC and is the correct length.

        // Try to get hold of the unit from the database.
        manufacturerId = fields[2];
        GotManufacturerId();
        if (!RtuExistsInDatabase())
        {
            ProtocolError(BulkProtocolHostErrorCode.SerialNumberNotRecognised);
            return;
        }
        // If we get here, we have the RTU ID, its PIN and its secret.

        // Check the CRC.
        if (!CheckCrc(line))
            return;
        // If we get here, the CRC matches - the unit appears to be who it says it is.

        // Check the timestamp; if it's unreadable or has drifted by more than our maximum allowed drift, correct it.
        DateTime timestamp = ParseTimestamp(fields[3]);
        if (!IsAcceptableTimestamp(timestamp))
        {
            // Log an RTC that's close to its reset value in case of power resets.
            if (timestamp <= MAX_RTC_FOR_RESET)
            {
                AddRtcReset(timestamp);
            }

            // Timestamps on any readings we receive will be dodgy.  Ignore them!
            ignoringReadingsAfterRtcReset = true;

            QueueSetRealTimeClock();
            QueueRequestConfigAndStatus();
        }

        // If we get here, there are no issues with the header.
        headerSeen = true;
    }

    private bool VerifyCrc(string line, string s)
    {
        if (!usesCrcs)
            return true;
        return Crc.VerifyCrc(line, s);
    }

    /// <summary>
    /// Returns the parsed UTC time from a Powelectrics-formatted date and time string, or DateTime.MinValue on a parse error.
    /// Units are always set in UTC.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    protected DateTime ParseTimestamp(string t)
    {
        if (!DateTime.TryParseExact(t, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime timestamp))
        {
            _logger.LogWarning("Date-Time received is invalid: {0}", t);
            return DateTime.MinValue;
        }
        return timestamp;
    }

    protected virtual bool IsAcceptableTimestamp(DateTime timestamp)
    {
        // Bad parses are never acceptable.
        if (DateTime.MinValue == timestamp)
            return false;

        // How many seconds of drift do we have?
        DateTime now = DateTime.UtcNow;
        long difference = (now.Ticks - timestamp.Ticks) / TimeSpan.TicksPerSecond;
        return !(Math.Abs(difference) > MAX_ALLOWED_RTC_DRIFT_S);
    }

    /// <summary>
    /// We're expecting a status line (starts with "S,").  Handle what we get.
    /// </summary>
    /// <param name="line"></param>
    protected virtual void ProcessStatusLine(string line)
    {
        if (!CheckCrc(line))
            return;

        string[] fields = line.Split(COMMA_ARRAY, StringSplitOptions.None);

        // A status line should have 8 or 9 fields (depending on whether the unit should CRC) and start with an "S".
        // If it's not a status line, there's a protocol error.  Log it and try to recover.
        if (!(fields.Length == 8 + (usesCrcs ? 1 : 0) && "S".Equals(fields[0])))
        {
            _logger.LogWarning("Cannot handle status line {Line} - wrong number of fields", line);
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        string mipString = fields[1];
        string failedCalloutString = fields[2];
        string temperatureString = fields[3];
        string battAlarmString = fields[4];
        string autoConfigString = fields[5];
        string carrier = fields[6];
        string signalString = fields[7];
            

        if (!TryParseZeroOne(mipString, out bool mip))
        {
            _logger.LogWarning("Cannot parse Status Line Mip - {Line}", line);
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        if (!TryParseZeroOne(failedCalloutString, out bool failedCallout))
        {
            _logger.LogWarning("Cannot parse Status Line FailedCallout - {Line}", line);
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        if (!TryParseZeroOne(battAlarmString, out bool battAlarm))
        {
            _logger.LogWarning("Cannot parse Status Line BattAlarm - {Line}", line);
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        if (!TryParseZeroOne(autoConfigString, out bool autoConfig))
        {
            _logger.LogWarning("Cannot parse Status Line AutoConfig - {Line}", line);
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        if (!double.TryParse(temperatureString, out double temperature))
        {
            _logger.LogWarning("Cannot parse Status Line Temperature - {Line}", line);
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        if (!double.TryParse(signalString, out double signal))
        {
            _logger.LogWarning("Cannot parse Status Line Signal - {Line}", line);
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        savedStatus = new Status { AutoConfig = autoConfig, BattAlarm = battAlarm, Carrier = carrier, FailedCallout = failedCallout, Mip = mip, Signal = signal, Temperature = temperature, DateRecordedUtc = DateTime.MinValue };

        AddConfigurationToTransactionIfAvailable();
    }

    /// <summary>
    /// The unit may not be fully configured.  If available, retrieve and queue a complete configuration for the unit.
    /// </summary>
    private void AddConfigurationToTransactionIfAvailable()
    {
        string rawConfiguration = null;
        try
        {
            using SqlConnection cn = Sql.GetSqlConnection();
            using SqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetConfigurationForRtu";
            cmd.Parameters.AddWithValue("@rtuId", rtuId);
            using SqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
            {
                rawConfiguration = r.GetString(0);
                pendingConfigurationUploadId = r.GetInt32(1);
            }
        }
         catch (SqlException ex)
        {
            _logger.LogWarning(ex, "Couldn't complete AddConfigurationToTransactionIfAvailable");
        }
        if (null != rawConfiguration)
            AddConfigurationToTransaction(rawConfiguration);
    }

    private void AddConfigurationToTransaction(string rawConfiguration)
    {
        // The M2 configurator tends to produce configurations with trailing commas on each line.  Remove these where present.
        Regex trailingCommaZapper = new(@",(?:\r?\n|$)");
        string processedConfiguration = trailingCommaZapper.Replace(rawConfiguration, "\r\n");

        using (TextReader tr = new StringReader(processedConfiguration))
        {
            string line = tr.ReadLine();
            while (null != line)
            {
                if (!string.IsNullOrEmpty(line))
                    transactionOutputQueue.Add(Formatting.FormatToSend(secret, line));
                line = tr.ReadLine();
            }
        }
        // Does this config change any sensors?  If so, we'll need to add a PIN,6 on the end to get new readings.
        try
        {
            using TextReader tr = new StringReader(processedConfiguration);
            Metron2ParserOutput config = Metron2ConfigurationParser.Parse(tr);
            if (config.Success && new SensorConfigurationChangeDetector().Detect(config.Configuration))
                QueueRequestConfigAndStatus();
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Couldn't parse/queue configuration ID: {RawConfiguration}", rawConfiguration);
        }
    }

    private void AddStatus(Status status)
    {
        if (null == savedStatus)
            return;

        {
            try
            {
                using SqlConnection cn = Sql.GetSqlConnection();
                using SqlCommand cmd = cn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AddStatusAndReadings";
                cmd.Parameters.AddWithValue("@rtuId", rtuId);
                cmd.Parameters.AddWithValue("@mip", status.Mip);
                cmd.Parameters.AddWithValue("@failedCallout", status.FailedCallout);
                cmd.Parameters.AddWithValue("@temperature", status.Temperature);
                cmd.Parameters.AddWithValue("@battAlarm", status.BattAlarm);
                cmd.Parameters.AddWithValue("@autoConfig", status.AutoConfig);
                cmd.Parameters.AddWithValue("@carrier", status.Carrier);
                cmd.Parameters.AddWithValue("@signal", status.Signal);
                cmd.Parameters.AddWithValue("@dateRecordedUtc", status.DateRecordedUtc == DateTime.MinValue ? DBNull.Value : (object)status.DateRecordedUtc);
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                PreserveAfterError(status, ex);
            }
        }
    }

    private void AddRtcReset(DateTime timestamp)
    {
        using SqlConnection cn = Sql.GetSqlConnection();
        using SqlCommand cmd = cn.CreateCommand();
        try
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AddRtcReset";
            cmd.Parameters.AddWithValue("@rtuId", rtuId);
            cmd.Parameters.AddWithValue("@incorrectValueUtc", DateTime.MinValue == timestamp ? new DateTime(1753, 1, 1) : timestamp);
            cmd.Parameters.AddWithValue("@dateRecordedUtc", DateTime.UtcNow);
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Couldn't log RTC reset");
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

    protected virtual void ProcessReadings(string line)
    {
        if (!CheckCrc(line))
            return;

        // The formatting of a L (or A, same format but Alarm) line is rather unpleasant to parse.  It uses commas to separate major fields, colons to separate readings (if more than one) and commas to separate parts of readings.
        // We deal with this by stripping off the tail of the line (always the CRC) if required and the head (just the L or A) and splitting the rest.
        int lastComma = usesCrcs ? line.LastIndexOf(',') : line.Length;
        int firstComma = line.IndexOf(',');
        if (firstComma < 0 || lastComma < 0 || firstComma == lastComma)
        {
            _logger.LogWarning("Cannot parse Reading {Line} - not enough commas", line);
            // Not enough commas in the line to get a reading
            ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
            return;
        }

        // If we get here, we have at least 0 readings (it could be L,[,CRC]).
        string lineForSplit = line.Substring(firstComma + 1, lastComma - firstComma - 1);
        // Format is reading:reading:reading, so split on those.
        string[] splitReadings = lineForSplit.Split(PERCENT_ARRAY);
        DateTime dateReceivedUtc = DateTime.UtcNow;
        bool isAlarm = line[0] == 'A' || line[0] == 'N';
        foreach (string splitReading in splitReadings)
        {
            // "Be conservative in what you send, liberal in what you accept." - W3C HTML recommendation
            // Ensure that empty readings (for example, from a unit sending too many %s) are discarded without binning acceptable readings.
            string trimmedReading = splitReading.Trim();
            if (string.IsNullOrEmpty(trimmedReading))
                continue;

            string[] readingParts = trimmedReading.Split(COMMA_ARRAY);
            // Format should be timestamp, sensor, value.
            if (readingParts.Length != 3)
            {
                _logger.LogWarning("Cannot parse Reading {Line} - wrong number of parts={Parts}", line, readingParts.Length);
                // Odd format
                ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
                return;
            }

            // We may not log the reading if we're ignoring them because the timestamp's dodgy.
            if (ignoringReadingsAfterRtcReset) continue;
                
            // Try to parse the reading; fail if we can't.  Mere impossible timestamps shouldn't force an error, as we should eventually send a RTC reset, but we shouldn't log the reading.
            if (!int.TryParse(readingParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int channel))
            {
                _logger.LogWarning("Cannot parse Reading {Line} - cannot parse channel {Channel}", line, readingParts[0]);
                // Can't parse one of the numbers
                ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
                return;
            }

            // If the timestamp's valid, we can finally log the reading.
            DateTime dateRecordedUtc = ParseTimestamp(readingParts[0]);
            if (dateRecordedUtc != DateTime.MinValue)
            {
                var valuePart= readingParts[2];
                if (TryParseNumber(valuePart, out var value))
                {
                    readings.Add(new NumericReading()
                    {
                        Channel = channel, 
                        DateRecordedUtc = dateRecordedUtc, 
                        Value = value, 
                        IsAlarm = isAlarm,
                        DateReceivedUtc = dateReceivedUtc
                    });
                }
                else if (TryParseHexValue(valuePart, out var hexValue))
                {
                    readings.Add(
                        new StringReading
                        {
                            Channel = channel,
                            DateRecordedUtc = dateRecordedUtc,
                            Value = hexValue,
                            IsAlarm = isAlarm,
                            DateReceivedUtc = dateReceivedUtc
                        });
                } 
                else if (TryParseStringValue(valuePart, out var stringValue))
                {
                    readings.Add(
                        new StringReading
                        {
                            Channel = channel, 
                            DateRecordedUtc = dateRecordedUtc, 
                            Value = stringValue,
                            IsAlarm = isAlarm, 
                            DateReceivedUtc = dateReceivedUtc
                        }
                    );
                }
                else
                {
                    _logger.LogWarning("Cannot parse Reading {Line} - wrong value format {Value}", line, valuePart);
                    ProtocolError(BulkProtocolHostErrorCode.IncorrectFormatOrContent);
                    return;
                }
            }
            // We log sensor readings for parts of our status; these are logged with the most recent date recorded, so that they match up with other readings.
            // If required, update the status appropriately.
            if (null != savedStatus && dateRecordedUtc > savedStatus.DateRecordedUtc)
                savedStatus.DateRecordedUtc = dateRecordedUtc;
        }
    }

    private static bool TryParseNumber(string valuePart, out double value) =>
        double.TryParse(valuePart, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    /// <summary>
    /// Parses `0x` followed by pairs of hex characters
    /// </summary>
    /// <remarks>This is case insensitive</remarks>
    /// <param name="value">The value to inspect</param>
    /// <param name="hexString">
    /// if returns <c>true</c> then the value with the `0x` prefix removed
    /// otherwise value is undefined
    /// </param>
    /// <returns><c>true</c> if <paramref name="value"/> represents a hex string</returns>
    private static bool TryParseHexValue(string value, out string hexString) 
    {
        hexString = null;
        if(!value.StartsWith("0x")) return false;
        if(value.Length % 2 != 0) return false;
        hexString = value.Substring(2);
        for(int i = 0, l = hexString.Length; i < l; i += 1) {
            if(!IsHexDigit(hexString[i])) return false;
        			
        }	
        return true;
    }

    private static bool IsHexDigit(char c) => c is >= 'a' and <= 'f' || c is >= 'A' and <= 'F' || c is >= '0' and <= '9';

    private static bool TryParseStringValue(string value, out string stringValue)
    {
        stringValue = null;
        if (value.Length <= 1) return false;
        if (!value.StartsWith('\"') || !value.EndsWith('\"')) return false;
        stringValue = value.Substring(1, value.Length - 2);
        return !(stringValue.Contains('\"') || stringValue.Contains(','));
    }

    /// <summary>
    /// An acknowledge has come in from the unit, and we were waiting for it.  Handle it.
    /// </summary>
    /// <param name="line"></param>
    protected virtual void HandleAck(string line)
    {
        // If the unit acked with a pending configuration, it acked the configuration - note success
        if (0 != pendingConfigurationUploadId)
            AddConfigurationUploadOutcome(true);

        // If we have more to send, send it.
        if (transactionOutputQueue.Count > 0)
        {
            MaybeSendTransaction();
            SetState(Metron2BulkTelemetryProcessorState.ExpectingAck);
        }
        else
        {
            // Otherwise, we're always unit-initiated, so the only scenarios where we get an ack with nothing more to do are when we have sent config or RTC.
            // If we've sent RTC, we will presently be ignoring readings.
            if (ignoringReadingsAfterRtcReset)
            {
                ignoringReadingsAfterRtcReset = false;
                SetState(Metron2BulkTelemetryProcessorState.ExpectingTransaction);
            }
            else 
            {
                // Nothing else to send or expect.  At this point, the only remaining thing is to kill the connection.
                KillConnection();
            }
        }
    }

    protected virtual void ProtocolError(BulkProtocolHostErrorCode errorCode)
    {
        ProtocolErrorHandler(errorCode);
        _logger.LogInformation("Error {Code}", errorCode);
        WriteLineImmediately("Err," + Convert.ToString((int)errorCode));
        EndConnection();
    }

    protected virtual void IllegalState()
    {
        IllegalStateHandler(state);
        // We've got into an illegal state.  Close the connection and give up.
        _logger.LogError("Illegal state encountered");
        EndConnection();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>PRE: id is set to the ID of the client.</remarks>
    private bool RtuExistsInDatabase()
    {
        using SqlConnection cn = Sql.GetSqlConnection();
        using SqlCommand cmd = cn.CreateCommand();
        try
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "LookupRtu";
            cmd.Parameters.AddWithValue("@manufacturerId", manufacturerId);
            using SqlDataReader r = cmd.ExecuteReader();
            if (r.Read())
            {
                rtuId = r.GetInt32(0);
                pin = r.GetString(1);
                secret = r.GetString(2);
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (SqlNullValueException)
        {
            return false;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    /// <summary>
    /// Add a reading into SQL Server.
    /// </summary>
    public void AddReading(NumericReading reading)
    {
        try
        {
            using SqlConnection cn = Sql.GetSqlConnection();
            using SqlCommand cmd = cn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "AddReading";
            cmd.Parameters.AddWithValue("@rtuId", rtuId);
            cmd.Parameters.AddWithValue("@channel", reading.Channel);
            cmd.Parameters.AddWithValue("@dateReceivedUtc", reading.DateReceivedUtc);
            cmd.Parameters.AddWithValue("@dateRecordedUtc", reading.DateRecordedUtc);
            cmd.Parameters.AddWithValue("@value", reading.Value);
            cmd.Parameters.AddWithValue("@isAlarm", reading.IsAlarm);
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            PreserveAfterError(reading, ex);
        }
    }

    public void AddReading(StringReading reading)
    {
        try
        {
            using SqlConnection cn = Sql.GetSqlConnection();
            using SqlCommand cmd = cn.CreateCommand();
            //cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "exec AddVarcharReading @rtuId, @channel, @dateReceivedUtc, @dateRecordedUtc, @value, @isAlarm";
            cmd.Parameters.AddWithValue("@rtuId", rtuId);
            cmd.Parameters.AddWithValue("@channel", reading.Channel);
            cmd.Parameters.AddWithValue("@dateReceivedUtc", reading.DateReceivedUtc);
            cmd.Parameters.AddWithValue("@dateRecordedUtc", reading.DateRecordedUtc);
            cmd.Parameters.AddWithValue("@value", reading.Value);
            cmd.Parameters.AddWithValue("@isAlarm", reading.IsAlarm);
            cmd.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            PreserveAfterError(reading, ex);
        }
    }

    private void QueueLine(string line)
    {
        _logger.LogDebug("> {0}", line);
        QueueBytes(encoding.GetBytes(line));
        QueueBytes(encoding.GetBytes("\n\r"));
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

    private void QueueFieldsWithPinAndCrc(params string[] fields)
    {
        string[] fieldsWithPin = new string[fields.Length + 1];
        fieldsWithPin[0] = pin;
        Array.Copy(fields, 0, fieldsWithPin, 1, fields.Length);
        string toSend = Formatting.FormatToSend(secret, fieldsWithPin);
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
                expected = (state & Metron2BulkTelemetryProcessorState.ConnectionErrorIsExpected) != 0;
            }
            if (!expected)
            {
                _logger.LogDebug(ex, "Issue writing bytes");
                EndConnection();
            }
        }
    }

    /// <summary>
    /// Callback from our timer once we've said goodbye.  If nothing else has closed the connection, we do so at this time.
    /// </summary>
    private void KillTimeout(object scrap)
    {
        try
        {
            lock (lockTarget)
            {
                if (state == Metron2BulkTelemetryProcessorState.WaitingAfterKill)
                    EndConnection();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Issue in KillTimeout");
        }
    }

    /// <summary>
    /// Callback from our watchdog timer.  Nothing has happened for a sufficiently long time that we want to kill this connection.
    /// </summary>
    private void WatchdogBark(object scrap)
    {
        lock (lockTarget)
        {
            _logger.LogDebug("Watchdog bark");
            EndConnection();
        }
    }

    /// <summary>
    /// The connection has finished, gone into an error state, or done something else that means we're not interested in it any more.
    /// Get rid of it, discarding any unsent data, and ensure this Reactor doesn't stay around.
    /// </summary>
    private void EndConnection()
    {
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
                tcpClient = null;
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
            savedStatus = null;
            SetState(Metron2BulkTelemetryProcessorState.Closed);
        }
    }

    private void QueueSetRealTimeClock()
    {
        // All dates and times sent to the RTU are UTC times.
        DateTime now = DateTime.UtcNow;
        QueueFieldsWithPinAndCrc("0", now.ToString("yyyyMMdd"), now.ToString("HH"), now.ToString("mm"));
    }

    private void QueueRequestConfigAndStatus()
    {
        if (!alreadyQueuedRequestConfigAndStatus)
        {
            alreadyQueuedRequestConfigAndStatus = true;
            QueueFieldsWithPinAndCrc("6");
            SetState(Metron2BulkTelemetryProcessorState.ExpectingTransaction);
        }
    }

    /// <summary>
    /// cmd has failed to run against the SQL database; keep it for later update.
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ex"></param>
    private void PreserveAfterError(object _, Exception ex)
    {
        lock (this)
        {
            _logger.LogWarning(ex, "SQL command exception");
            // TODO: Log the object.  This was removed by PJC 2019-09-22 because the log is, in reality, never used.
        }
    }

    private void SetState(Metron2BulkTelemetryProcessorState newState)
    {
        state = newState;
    }

    public void Dispose()
    {
        killWait?.Dispose();
        watchdog?.Dispose();
        queuedBytes?.Dispose();
        networkStream?.Dispose();
        tcpClient?.Dispose();
    }
}