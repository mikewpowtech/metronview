namespace Metron2Configuration
{
    /// <summary>
    /// The main configuration of channels: a name
    /// </summary>
    public abstract class NamedChannelConfiguration : ChannelConfiguration
    {
        public string Name { get; set; }
        public override bool IsPrimaryConfiguration => true;
    }
}
