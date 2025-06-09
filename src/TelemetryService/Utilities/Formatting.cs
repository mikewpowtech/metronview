namespace Utilities
{
    public class Formatting
    {
        public static string FormatToSend(string secret, params string[] strings)
        {
            string message = string.Join(",", strings);
            string crc = Crc.CalculatePrintableCrc(message, secret);
            message += "," + crc;
            return message;
        }
    }
}
