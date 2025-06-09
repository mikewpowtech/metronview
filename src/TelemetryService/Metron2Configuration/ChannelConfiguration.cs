using System.Collections.Generic;

namespace Metron2Configuration
{
    /// <summary>
    /// Holds a channel configuration - what a pin,3 sets.
    /// </summary>
    public abstract class ChannelConfiguration : IVisitableConfiguration
    {
        public int Channel { get; set; }
        public virtual bool IsPrimaryConfiguration => false;
        public abstract IList<PrimaryChannelType> CompatibleChannelTypes { get; }
        public abstract void Accept(IConfigurationVisitor visitor);
    }
}
