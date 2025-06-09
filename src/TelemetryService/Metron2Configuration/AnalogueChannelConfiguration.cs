using System.Collections.Generic;

namespace Metron2Configuration
{
    public class AnalogueChannelConfiguration : NamedChannelConfiguration
    {
        public const InputType DIGITAL_INPUT_TYPE = InputType.V0_10;
        public const ExcitationVoltage DIGITAL_EXCITATION_VOLTAGE = ExcitationVoltage.V5;
        public const string DIGITAL_ENGINEERING_UNITS = "Digital";

        /// <summary>
        /// The transition voltage where a digital channel is assumed to move from undefined to high
        /// </summary>
        public const int THRESHOLD_V_UP = 3; // Volts

        /// <summary>
        /// The transition voltage where a digital channel is assumed to move from undefined to low
        /// </summary>
        public const int THRESHOLD_V_DOWN = 2; // Volts

        public ExcitationVoltage ExcitationVoltage { get; set; }
        public InputType InputType { get; set; }
        public int SettleTime { get; set; }
        public double Zero { get; set; }
        public double Span { get; set; }
        public string EngineeringUnits { get; set; }
        public int Hysteresis { get; set; }
        public double LoLoAlarm { get; set; }
        public double LoAlarm { get; set; }
        public double HiAlarm { get; set; }
        public double HiHiAlarm { get; set; }
        public int CalloutDelay { get; set; }

        public override IList<PrimaryChannelType> CompatibleChannelTypes =>
            new PrimaryChannelType[] {PrimaryChannelType.Analogue};

        public override void Accept(IConfigurationVisitor visitor)
        {
            visitor.Visit(this);
        }

        public bool IsDigital()
        {
            return InputType == InputType.V0_10 && ExcitationVoltage == ExcitationVoltage.V5 &&
                   "Digital".Equals(EngineeringUnits);
        }
    }
}
