using System;
using System.Linq;
using System.Text;
using Metron2Configuration;

namespace Metron2Parser
{
    public class Metron2ConfigurationRenderer : IConfigurationVisitor
    {
        private StringBuilder sb;

        public string Render(Configuration victim)
        {
            sb = new StringBuilder();
            victim.Accept(this);
            return sb.ToString();
        }

        public string Render(ChannelConfigurationList victim)
        {
            sb = new StringBuilder();
            victim.Accept(this);
            return sb.ToString();
        }

        void IConfigurationVisitor.Visit(Configuration element)
        {
            if (element.ResetUnit)
                RenderLine("5");
        }

        void IConfigurationVisitor.Visit(SystemConfiguration config)
        {
            // There's a "feature" in Metron 2s: the transmit interval is off by one, so a value of 59 sets a Tx interval of 1 hour (60 minutes).  Hence the "TxInterval - 1" below.
            if (null != config)
                RenderLine("2", config.Name, RenderHour(config.TxTime), RenderMinute(config.TxTime), Render(config.Variance), Render(config.TxInterval - 1), Render(config.WakeupInterval), Render(config.TemperatureEnabled), Render(config.UseExternalAntenna), Render(config.Idle), Render(config.ReadOnWake), Render(config.Log), Render(config.BatteryAlarmPercent), Render(config.Format));
        }

        public void Visit(ChannelsConfiguration element)
        {
            // Nothing required; we render each element below.
        }

        public void Visit(ChannelConfigurationList element)
        {
            // Nothing required; we render each element below.
        }

        void IConfigurationVisitor.Visit(AnalogueChannelConfiguration element)
        {
            RenderChannelLine(element.Channel, "a", new string[] { element.Name, Render(element.ExcitationVoltage), Render(element.SettleTime), element.EngineeringUnits, Render(element.Zero), Render(element.Span), Render(element.LoLoAlarm), Render(element.LoAlarm), Render(element.HiAlarm), Render(element.HiHiAlarm), Render(element.Hysteresis), Render(element.CalloutDelay), Render(element.InputType) } );
        }

        void IConfigurationVisitor.Visit(DisableChannelConfiguration element)
        {
            RenderChannelLine(element.Channel, "x");
        }

        public void Visit(LineariseChannelConfiguration element)
        {
            RenderChannelLine(element.Channel, "c", element.Values.Select(d => d.ToString()).Take(32).ToArray());
        }

        private string Render(int? i)
        {
            if (!i.HasValue)
                return string.Empty;
            return i.ToString();
        }

        private string Render(double? i)
        {
            if (!i.HasValue)
                return string.Empty;
            // Force a value without exponent and without group separators.  Assumes a "UK-ish" group separator of comma, and no more than 99 digits after the decimal point.
            return i.Value.ToString("F99").Replace(",", "").TrimEnd('0').TrimEnd('.');
        }

        private string Render(bool? b)
        {
            if (!b.HasValue)
                return string.Empty;
            return b.Value ? "1" : "0";
        }

        private string RenderHour(DateTime? time)
        {
            if (!time.HasValue)
                return string.Empty;
            return time.Value.Hour.ToString("00");
        }

        private string RenderMinute(DateTime? time)
        {
            if (!time.HasValue)
                return string.Empty;
            return time.Value.Minute.ToString("00");
        }

        private string Render(ExcitationVoltage excitationVoltage)
        {
            return ((int)excitationVoltage).ToString();
        }

        private string Render(InputType inputType)
        {
            return ((int)inputType).ToString();
        }

        private void RenderChannelLine(int channel, string subcommand, string[] commaSeparatedValues = null)
        {
            string renderedChannel = 0 == channel ? "{channel}" : channel.ToString();
            if (null == commaSeparatedValues)
                RenderLine("3", renderedChannel, subcommand);
            else
                RenderLine(new string[] { "3", renderedChannel, subcommand }.Concat(commaSeparatedValues).ToArray());
        }

        private void RenderLine(params string[] commaSeparatedValues)
        {
            sb.Append("{pin},");
            foreach (string value in commaSeparatedValues)
            {
                sb.Append(value);
                sb.Append(",");
            }
            sb.AppendLine();
        }
    }
}
