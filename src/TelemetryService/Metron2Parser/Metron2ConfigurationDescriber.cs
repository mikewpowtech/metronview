using System.Text;
using Metron2Configuration;

namespace Metron2Parser
{
    /// <summary>
    /// A visitor that produces an English-language description of whatever it is pointed at.
    /// To use: Call Render on the appropriate element.
    /// </summary>
    public class Metron2ConfigurationDescriber : IConfigurationVisitor
    {
        private StringBuilder sb;
        private string channelBulletPrefix;

        public string Render(Configuration victim)
        {
            channelBulletPrefix = "- ";
            sb = new StringBuilder();
            if (null != victim)
                victim.Accept(this);
            return sb.ToString();
        }

        public string Render(ChannelConfigurationList victim)
        {
            channelBulletPrefix = string.Empty;
            sb = new StringBuilder();
            if (null != victim)
                victim.Accept(this);
            return sb.ToString();
        }

        void IConfigurationVisitor.Visit(Configuration element)
        {
            if (element.ResetUnit)
                sb.AppendLine("Reset unit.");
        }

        void IConfigurationVisitor.Visit(SystemConfiguration config)
        {
            if (null != config)
            {
                sb.AppendLine($"System name '{config.Name}', reference time {config.TxTime:HH:mm} +/- {config.Variance} minutes. {(config.UseExternalAntenna ? "external" : "internal")} antenna, {(config.ReadOnWake ? "take readings" : "show menu")} when button pressed.");
                sb.AppendLine($"Wake every {config.WakeupInterval} minutes, transmit every {config.TxInterval} minutes. Between transmissions, {IdleToEnglish(config.Idle)}.");
            }
        }

        private object IdleToEnglish(int? nullable)
        {
            if (!nullable.HasValue)
                return "unknown action";
            switch(nullable.Value)
            {
                case 0:
                    return "power down modem";
                case 1:
                    return "keep modem on and GPRS pollable";
                case 2:
                    return "keep modem on and SMS pollable";
                case 3:
                    return "Transmit on power up, keep modem on and SMS pollable";
                case 4:
                    return "Transmit on power up, keep modem on and GPRS pollable";
                default:
                    return "Unknown action";
            }
        }

        void IConfigurationVisitor.Visit(ChannelsConfiguration element)
        {
            // Nothing required
        }

        void IConfigurationVisitor.Visit(ChannelConfigurationList element)
        {
            if (Channel && 0 != element.Channel)
                AddChunk($"Channel {element.Channel.Value}:");
        }

        private void AddChunk(string chunk)
        {
            sb.Append(chunk);
            if (Multiline)
                sb.AppendLine();
            else
                sb.Append(' ');
        }

        void IConfigurationVisitor.Visit(AnalogueChannelConfiguration element)
        {
            bool isDigital = element.IsDigital();
            if (isDigital)
            {
                string triggerOn;
                if (element.HiAlarm > 0)
                {
                    if (element.LoAlarm > 0)
                        triggerOn = "both";
                    else
                        triggerOn = "open to closed";
                }
                else
                {
                    if (element.LoAlarm > 0)
                        triggerOn = "closed to open";
                    else
                        triggerOn = "nothing (value will be transmitted on schedule)";
                }
                AddChunk($"{channelBulletPrefix}Digital, transmit on {triggerOn}.");
            }
            else
            {
                AddChunk($"{channelBulletPrefix}Analogue. '{element.Name}'. Excite with {Render(element.ExcitationVoltage)}, wait {element.SettleTime} seconds then sample as {Render(element.InputType)} and treat as {element.Zero}-{element.Span} {element.EngineeringUnits}.");
                bool first = true;
                MaybeAddAlarm(ref first, "Low-low", element.LoLoAlarm);
                MaybeAddAlarm(ref first, "Low", element.LoAlarm);
                MaybeAddAlarm(ref first, "High", element.HiAlarm);
                MaybeAddAlarm(ref first, "High-high", element.HiHiAlarm);
                AddChunk(".");
            }
        }

        private void MaybeAddAlarm(ref bool first, string prefix, double? alarmValue)
        {
            if (alarmValue.HasValue)
            {
                if (first)
                    sb.Append(channelBulletPrefix);
                else
                    sb.Append(", ");
                sb.Append($"{(first ? prefix : prefix.ToLower())} alarm at {alarmValue}");
                first = false;
            }
        }

        void IConfigurationVisitor.Visit(DisableChannelConfiguration element)
        {
            AddChunk($"{channelBulletPrefix}Disabled.");
        }

        public void Visit(LineariseChannelConfiguration element)
        {
            AddChunk($"{channelBulletPrefix}Linearised.");
        }

        private string Render(ExcitationVoltage excitationVoltage)
        {
            switch (excitationVoltage)
            {
                case ExcitationVoltage.Battery:
                    return "battery voltage";
                case ExcitationVoltage.V21_6:
                    return "21.6V";
                case ExcitationVoltage.V5:
                    return "5V";
                default:
                    return "unknown";
            }
        }

        private string Render(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.MA0_20:
                    return "0-20mA";
                case InputType.MA4_20:
                    return "4-20mA";
                case InputType.V0_10:
                    return "0-10V";
                default:
                    return "unknown";
            }
        }

        public bool Multiline { get; set; }

        public bool Channel { get; set; }
    }
}
