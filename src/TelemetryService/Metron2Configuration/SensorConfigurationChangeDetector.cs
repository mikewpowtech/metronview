namespace Metron2Configuration
{
    /// <summary>
    /// A visitor that returns true iff the configuration it's pointed at changes a sensor configuration such that readings might change.
    /// To use: Call Render on the appropriate element.
    /// </summary>
    public class SensorConfigurationChangeDetector : IConfigurationVisitor
    {
        private bool changesSensorConfiguration; // = false
        public bool Detect(Configuration victim)
        {
            if (null != victim)
                victim.Accept(this);
            return changesSensorConfiguration;
        }

        void IConfigurationVisitor.Visit(Configuration element)
        {
            // Nothing required
        }

        void IConfigurationVisitor.Visit(SystemConfiguration config)
        {
            // Nothing required
        }

        void IConfigurationVisitor.Visit(ChannelsConfiguration element)
        {
            // Nothing required
        }

        void IConfigurationVisitor.Visit(ChannelConfigurationList element)
        {
            // Nothing required
        }

        void IConfigurationVisitor.Visit(AnalogueChannelConfiguration element)
        {
            changesSensorConfiguration = true;
        }

        void IConfigurationVisitor.Visit(DisableChannelConfiguration element)
        {
            // Nothing required
        }

        public void Visit(LineariseChannelConfiguration element)
        {
            changesSensorConfiguration = true;
        }
    }
}
