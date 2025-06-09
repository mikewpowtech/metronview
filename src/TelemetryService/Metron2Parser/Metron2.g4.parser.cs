using System;
using Metron2Configuration;

namespace Metron2Parser
{
    partial class Metron2Parser
    {
        private DateTime MkTimeOfDayUtc(int hour, int minute)
        {
            return new DateTime(1900, 1, 1, hour, minute, 0, DateTimeKind.Utc);
        }

        private ExcitationVoltage ToExcitationVoltage(int value)
        {
            if (value < 0 || value > 2)
                throw new ArgumentOutOfRangeException("value", value, "Excitation voltage type must be between 0 and 2 inclusive");
            return (ExcitationVoltage)value;
        }

        private InputType ToInputType(int value)
        {
            if (value < 0 || value > 2)
                throw new ArgumentOutOfRangeException("value", value, "Input type must be between 0 and 2 inclusive");
            return (InputType)value;
        }
    }
}
