namespace Metron2Configuration
{
    public interface IConfigurationVisitor
    {
        void Visit(Configuration element);
        void Visit(SystemConfiguration element);
        void Visit(ChannelsConfiguration element);
        void Visit(ChannelConfigurationList element);
        void Visit(AnalogueChannelConfiguration element);
        void Visit(DisableChannelConfiguration element);
        void Visit(LineariseChannelConfiguration element);
    }
}
