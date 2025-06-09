using System;

namespace Metron2Configuration
{
    /// <summary>
    /// Holds the system configuration of a Metron 2.  This is essentially what a pin,2 sets.
    /// </summary>
    public class SystemConfiguration : IVisitableConfiguration
    {
        public bool UseExternalAntenna { get; set; }
        public int BatteryAlarmPercent { get; set; }
        /// <summary>
        /// Output format: 0 = human readable, 2 = gateway, 3 = TODO: check.
        /// </summary>
        public int Format => 2;
        public int Idle { get; set; }
        public int Log { get; set; }
        public string Name { get; set; }
        public bool ReadOnWake { get; set; }
        public bool TemperatureEnabled { get; set; }
        /// <summary>
        /// This is the TxInternal + 1 (eg 1440=>1439)
        /// </summary>
        public int TxInterval { get; set; }
        public DateTime TxTime { get; set; }
        public int Variance { get; set; }
        public int WakeupInterval { get; set; }

        /// <summary>
        /// Amend this configuration with any new pieces in toMerge, which will overwrite existing configuration in this.
        /// </summary>
        /// <param name="toMerge"></param>
        public void Merge(SystemConfiguration toMerge)
        {
            if (null == toMerge)
                return; // Nothing to do

            UseExternalAntenna = toMerge.UseExternalAntenna;
            BatteryAlarmPercent = toMerge.BatteryAlarmPercent;
            Idle = toMerge.Idle;
            Log = toMerge.Log;
            if (null != toMerge.Name)
                Name = toMerge.Name;
            ReadOnWake = toMerge.ReadOnWake;
            TemperatureEnabled = toMerge.TemperatureEnabled;
            TxInterval = toMerge.TxInterval;
            TxTime = toMerge.TxTime;
            WakeupInterval = toMerge.WakeupInterval;
        }

        public void Accept(IConfigurationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
