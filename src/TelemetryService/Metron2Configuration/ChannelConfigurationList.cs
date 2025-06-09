using System.Collections.Generic;

namespace Metron2Configuration
{
    public class ChannelConfigurationList : List<ChannelConfiguration>, IVisitableConfiguration
    {
        public ChannelConfigurationList()
        {
        }

        public ChannelConfigurationList(IEnumerable<ChannelConfiguration> collection) : base(collection)
        {
        }

        public void Accept(IConfigurationVisitor visitor)
        {
            visitor.Visit(this);
            foreach (ChannelConfiguration victim in this)
                victim.Accept(visitor);
        }

        /// <summary>
        /// Set the channel of all configuration elements within this list; or get the channel of the first (which should always be representative of all of them), or null if there are no entries from which to retrieve the channel.
        /// </summary>
        public int? Channel
        {
            get
            {
                if (Count == 0)
                    return null;
                return this[0].Channel;
            }
            set
            {
                if (!value.HasValue)
                    return;
                foreach (ChannelConfiguration config in this)
                    config.Channel = value.Value;
            }
        }

        public string Name
        {
            get
            {
                foreach (ChannelConfiguration x in this)
                    if (x.IsPrimaryConfiguration && x is NamedChannelConfiguration)
                        return ((NamedChannelConfiguration)x).Name;
                return null;
            }
            set
            {
                foreach (ChannelConfiguration x in this)
                    if (x.IsPrimaryConfiguration && x is NamedChannelConfiguration)
                        ((NamedChannelConfiguration)x).Name = value;
            }
        }
    }
}
