using System.Collections.Generic;

namespace Metron2Configuration
{
    public class LineariseChannelConfiguration : ChannelConfiguration
    {
        public List<double?> Values { get; }
        public override IList<PrimaryChannelType> CompatibleChannelTypes => new PrimaryChannelType[] { PrimaryChannelType.Analogue };

        public LineariseChannelConfiguration()
        {
            Values = new List<double?>();
        }

        public override void Accept(IConfigurationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
