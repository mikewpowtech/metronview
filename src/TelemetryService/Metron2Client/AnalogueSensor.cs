namespace Metron2Client;

public class AnalogueSensor : ISensor
{
    public int Channel { get; }
    public bool IsDigital => false;
    public string Name { get; }
    public double Value { get; set; }

    public AnalogueSensor(int channel, string name)
    {
        Channel = channel;
        Name = name;
    }
}
