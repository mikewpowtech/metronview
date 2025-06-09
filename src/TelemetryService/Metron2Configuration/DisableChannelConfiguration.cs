using System.Collections.Generic;

namespace Metron2Configuration
{
    public class DisableChannelConfiguration : ChannelConfiguration
    {
        public override bool IsPrimaryConfiguration => true;
        public override IList<PrimaryChannelType> CompatibleChannelTypes => new PrimaryChannelType[] { PrimaryChannelType.Disabled };

        public override void Accept(IConfigurationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
