using System;

namespace Metron2Client;

public class RandomAnalogueSensor : ISensor
{
    public int Channel { get; }
    public bool IsDigital => false;
    public string Name { get; }
    private readonly Random random;

    public RandomAnalogueSensor(int channel, string name, Random random)
    {
        Channel = channel;
        Name = name;
        this.random = random;
    }

    public double Value => (random.NextDouble() + Channel) * 5000;
}
