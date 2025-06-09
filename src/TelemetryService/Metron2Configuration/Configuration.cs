using System;
using System.Linq;

namespace Metron2Configuration
{
    /// <summary>
    /// Holds a configuration of a Metron 2.
    /// </summary>
    public class Configuration : IVisitableConfiguration
    {
        public bool ResetUnit { get; set; }
        public SystemConfiguration SystemConfiguration { get; set; }
        public ChannelsConfiguration ChannelsConfiguration { get; set; }

        public Configuration()
        {
            ChannelsConfiguration = new ChannelsConfiguration();
        }

        /// <summary>
        /// Amend this configuration with any new pieces in toMerge, which will overwrite existing configuration in this.
        /// </summary>
        /// <param name="toMerge"></param>
        public void Merge(Configuration toMerge)
        {
            if (null == toMerge)
                return; // Nothing to do

            ResetUnit = toMerge.ResetUnit;
            MergeSystemConfiguration(toMerge.SystemConfiguration);
            MergeChannelsConfiguration(toMerge.ChannelsConfiguration);
        }

        private void MergeChannelsConfiguration(ChannelsConfiguration toMerge)
        {
            if (null == ChannelsConfiguration)
                ChannelsConfiguration = toMerge;
            else
                ChannelsConfiguration.Merge(toMerge);
        }

        private void MergeSystemConfiguration(SystemConfiguration toMerge)
        {
            if (null == SystemConfiguration)
                SystemConfiguration = toMerge;
            else
                SystemConfiguration.Merge(toMerge);
        }

        public void Accept(IConfigurationVisitor visitor)
        {
            visitor.Visit(this);
            if (null != SystemConfiguration)
                SystemConfiguration.Accept(visitor);
            if (null != ChannelsConfiguration)
            {
                int[] sortedChannels = ChannelsConfiguration.Keys.ToArray();
                Array.Sort(sortedChannels);
                foreach (int channel in sortedChannels)
                {
                    ChannelConfigurationList victims;
                    if (null != ChannelsConfiguration[channel] && ChannelsConfiguration.TryGetValue(channel, out victims))
                        victims.Accept(visitor);
                }
            }
        }
    }
}
