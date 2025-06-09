using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using NLog;
using Utilities;

namespace Metron2Client;

public class Metron2
{
    private const int MAX_TIME_OFFSET_MSEC = 100000;
    private static readonly Random rnd = new Random();
    private static readonly byte[] CR_LF_BYTES = { 13, 10 };

    // RTU variables
    private readonly string uniqueId;
    private int timeOffsetInMsec;
    public string Secret { get; set; }
    public string Pin { get; set; }
    private bool mip;
    private bool failedCallout;
    private bool tempEnabled = true;
    private bool battAlarm;
    private bool autoConfig;
    private string carrier;
    private readonly string hostname;
    private readonly int port;
    private readonly IList<ISensor> sensors;

    // Connection variables
    private bool connected;
    private TcpClient client;
    private NetworkStream stream;
    readonly IList<string> transactionInputData;

    /// <summary>
    /// Convenience array for string.Split(), when splitting lists on commas.
    /// </summary>
    private static readonly char[] COMMA_ARRAY = { ',' };

    // Misc
    static readonly Logger nlogger = LogManager.GetCurrentClassLogger();

    public Metron2(string uniqueId, string hostname, int port)
    {
        this.uniqueId = uniqueId;
        timeOffsetInMsec = 0; // (int)(rnd.NextDouble() * (2.0 * MAX_TIME_OFFSET_MSEC)) - MAX_TIME_OFFSET_MSEC;
        Secret = "";
        battAlarm = false;
        mip = false;
        failedCallout = false;
        carrier = "Internet";
        this.hostname = hostname;
        this.port = port;
        sensors = new List<ISensor>();
        transactionInputData = new List<string>();
    }

    public bool AutoConfig
    {
        get => autoConfig;
        set => autoConfig = value;
    }

    public void AddSensor(ISensor sensor)
    {
        sensors.Add(sensor);
    }

    public void OneIteration()
    {
        try
        {
            StartBlock();
            SendGreeting();
            if (connected)
            {
                SendStatus();
                SendReadings(false);
                EndBlock();
                bool carryOn;
                do
                {
                    carryOn = ExpectBlockOrKill();
                } while (carryOn);
            }
            else
            {
                nlogger.Trace("Couldn't connect");
            }
        }
        catch (Exception ex)
        {
            nlogger.Warn(ex, "Exception during OneIteration");
        }
    }

    public void ConnectAndSendReadings(IList<NumericReading> readings, bool isAlarm)
    {
            StartBlock();
            SendGreeting();
            if (connected)
            {
                SendStatus();
                SendReadings(readings, isAlarm);
                EndBlock();
                bool carryOn;
                do
                {
                    carryOn = ExpectBlockOrKill();
                } while (carryOn);
            }
    }

    private void SendGreeting()
    {
        // The first two fields of the header are not part of the CRC (for some reason).
        Send("POWE", "METRON2AP", uniqueId, Timestamp("yyyyMMddHHmmss"));
    }

    private void SendStatus()
    {
        Send("S", mip ? "1" : "0", failedCallout ? "1" : "0", tempEnabled ? Temperature() : "", battAlarm ? "1" : "0", autoConfig ? "1" : "0", carrier, Signal());
    }

    private IList<NumericReading> GenerateReadings()
    {
        IList<NumericReading> readings = new List<NumericReading>();
        foreach (ISensor sensor in sensors)
        {
            NumericReading numericReading = new NumericReading
            {
                Channel = sensor.Channel,
                DateRecordedUtc = DateTime.UtcNow,
                Value = sensor.Value
            };
            readings.Add(numericReading);
        }
        return readings;
    }

    private void SendReadings(bool isAlarm)
    {
        SendReadings(GenerateReadings(), isAlarm);
    }

    private void SendReadings(IList<NumericReading> readings, bool isAlarm)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(isAlarm ? "A," : "L,");
        bool first = true;
        foreach (NumericReading reading in readings)
        {
            if (first)
                first = false;
            else
                sb.Append("%");
            sb.Append(reading.DateRecordedUtc.ToString("yyyyMMddHHmmss"));
            sb.Append(",");
            sb.Append(reading.Channel.ToString("N0", CultureInfo.InvariantCulture));
            sb.Append(",");
            string formattedValue = reading.Value.ToString(CultureInfo.InvariantCulture);
            sb.Append(formattedValue.Substring(0, Math.Min(8, formattedValue.Length)));
        }
        string preCrc = sb.ToString();
        sb.Append(",");
        sb.Append(Crc.CalculatePrintableCrc(preCrc, Secret));
        SendALine(sb.ToString());
    }

    private string Timestamp(string format)
    {
        DateTime now = DateTime.UtcNow;
        now = now.AddMilliseconds(timeOffsetInMsec);
        return now.ToString(format);
    }

    private string Signal()
    {
        return rnd.Next(3, 41).ToString();
    }

    private string Temperature()
    {
        return (rnd.NextDouble() * 40.0).ToString("N2", CultureInfo.InvariantCulture);
    }

    private void StartBlock()
    {
        // Do nothing
    }

    private void EndBlock()
    {
        SendALine("E");
        ExpectAcknowledge();
    }

    private void ExpectAcknowledge()
    {
        string data = ReadALine();
        // Strip any terminating CRLF
        data = data.TrimEnd('\n', '\r');
        // Should be either an A or an error.  Anything else and the connection is corrupt, so kill it.
        if ("A".Equals(data))
        {
            // Successful acknowledge
            return;
        }

        // Error, so drop the connection
        Disconnect();

        // If it's a properly formatted error, keep it.
        if (data.StartsWith("Err,"))
        {
            throw new Exception($"Error returned: {data}");
        }
        else
        {
            throw new Exception("Unexpected server response: {data}");
        }
    }

    private bool ExpectBlockOrKill()
    {
        while (true)
        {
            string data = ReadALine();
            // Strip any terminating CRLF
            data = data.TrimEnd('\n', '\r');

            // A kill stops this iteration.
            if ("K".Equals(data))
            {
                Disconnect();
                return false;
            }

            // An acknowledge is unexpected
            if ("A".Equals(data))
            {
                // TODO: ProtocolError
                return false;
            }

            // An error is unexpected
            if (data.StartsWith("Err,"))
            {
                // TODO: ProtocolError
                return false;
            }

            // An E causes the block to be processed
            if ("E".Equals(data))
            {
                ProcessTransaction();
                return true;
            }

            // Anything else adds to the transaction
            transactionInputData.Add(data);
        }
    }

    private void ProcessTransaction()
    {
        SendALine("A");
        foreach (string command in transactionInputData)
            ProcessCommand(command);
        transactionInputData.Clear();
    }

    private void ProcessCommand(string command)
    {
        string[] fields = command.Split(COMMA_ARRAY);
        // TODO: Check PIN and CRC

        if (fields.Length >= 2)
        {
            if ("0".Equals(fields[1]))
                ProcessSetRealTimeClock(command);
            else if ("6".Equals(fields[1]))
                ProcessRequestConfigAndStatus();
        }
    }

    private void ProcessSetRealTimeClock(string command)
    {
        // TODO: Do it properly
        timeOffsetInMsec = 0;
    }

    private void ProcessRequestConfigAndStatus()
    {
        SendGreeting();
        SendStatus();
        SendReadings(false);
        EndBlock();
    }

    private void SendWithNonCrcPrefix(string prefix, params string[] strings)
    {
        string message = Formatting.FormatToSend(Secret, strings);
        SendALine(prefix + message);
    }

    private void Send(params string[] strings)
    {
        string message = Formatting.FormatToSend(Secret, strings);
        SendALine(message);
    }

    private void SendALine(string message)
    {
        if (!connected)
            ConnectToServer();
        if (connected)
        {
            WriteBytes(Encoding.ASCII.GetBytes(message));
            WriteBytes(CR_LF_BYTES);
        }
    }

    private void ConnectToServer()
    {
        if (connected)
            throw new Exception("Trying to connect when already connected");
        client = new TcpClient(hostname, port);
        stream = client.GetStream();
        connected = true;
    }

    private void Disconnect()
    {
        if (connected)
        {
            stream.Dispose();
            stream = null;
            client.Close();
            client = null;
            connected = false;
        }
    }

    private void WriteBytes(byte[] bytes)
    {
        if (!connected)
            throw new Exception("Trying to write bytes when disconnected");
        stream.Write(bytes, 0, bytes.Length);
    }

    private string ReadALine()
    {
        if (!connected)
            throw new Exception("Trying to read bytes when disconnected");
        StringBuilder sb = new StringBuilder();
        byte[] ba = new byte[1];
        while (true)
        {
            int i = stream.ReadByte();
            if (i < 0)
            {
                break;
            }
            ba[0] = (byte)i;
            char[] ca = Encoding.ASCII.GetChars(ba);
            sb.Append(ca);
            // ASSUME: We happen to know there will only ever be one byte in ba, an ASCII encoding, and hence one byte in ca.  So this is guaranteed to examine every character.
            // If the encoding changes, re-check this assumption!
            if ('\r' == ca[0])
                break;
        }
        return sb.ToString();
    }
}
